using DollarExchangeRateService.Persistence;
using DollarExchangeRateService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DollarExchangeRateService.HostedServices
{
    internal class ExchangeRateCollectorService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IOptions<AppConfig> _appConfig;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IExchangeRateStorage _exchangeRateStorage;
        private Timer _timer;

        public ExchangeRateCollectorService(ILogger<ExchangeRateCollectorService> logger, IOptions<AppConfig> appConfig,
            IExchangeRateService exchangeRateService, IExchangeRateStorage exchangeRateStorage)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _exchangeRateStorage = exchangeRateStorage ?? throw new ArgumentNullException(nameof(exchangeRateStorage));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting");
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromHours(_appConfig.Value.DataCollectorIntervalInHours));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            var policy = Policy
                .Handle<WebException>()
                .WaitAndRetryAsync(_appConfig.Value.MaxRetryAttempts,
                    retryAttempt =>
                        TimeSpan.FromSeconds(
                            Math.Pow(_appConfig.Value.FirstPauseBetweenFailuresInSec, retryAttempt)),
                    (exception, retryCount) =>
                    {
                        _logger.LogError(
                            $"An error has occurred while retrieving data from {nameof(ExchangeRateService)} service: {exception}");
                    });
            try
            {
                await policy
                    .ExecuteAsync(async () => await AddNewExchangeRateEntry().ConfigureAwait(false))
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError($"An error has occurred: {e}");
            }
        }

        private async Task AddNewExchangeRateEntry()
        {
            _logger.LogInformation("Retrieving a new entry");
            var lastEntry = await _exchangeRateService.GetLastAsync().ConfigureAwait(false);
            _logger.LogInformation("Saving a new entry");
            await _exchangeRateStorage.SaveAsync(lastEntry).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}