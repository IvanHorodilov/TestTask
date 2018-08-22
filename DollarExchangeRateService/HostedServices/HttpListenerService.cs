using DollarExchangeRateService.Extensions;
using DollarExchangeRateService.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DollarExchangeRateService.HostedServices
{
    internal class HttpListenerService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IExchangeRateStorage _exchangeRateStorage;
        private readonly IOptions<AppConfig> _appConfig;
        private HttpListener _httpListener;

        public HttpListenerService(ILogger<HttpListenerService> logger,
            IExchangeRateStorage exchangeRateStorage, IOptions<AppConfig> appConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _exchangeRateStorage = exchangeRateStorage ?? throw new ArgumentNullException(nameof(exchangeRateStorage));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting");
            DoWork(cancellationToken).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(@"http://+:80/");
            try
            {
                _httpListener.Start();
            }
            catch (HttpListenerException e)
            {
                _logger.LogError($"Check if you are under administrator rights, {e}");
                await StopAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }

            _logger.LogInformation($"Waiting for connections");
            while (true)
            {
                var context = await _httpListener.GetContextAsync().ConfigureAwait(false);
                _logger.LogInformation($"Received a request from {context.Request.RemoteEndPoint}");

                var response = context.Response;
                response.ContentType = "application/json";

                string responseString;
                try
                {
                    var currentStatus = await _exchangeRateStorage
                        .GetCurrentStatusAsync(_appConfig.Value.DaysToUseForMovingAverage).ConfigureAwait(false);
                    responseString = Serialize(currentStatus);
                }
                catch (NoDataFoundInStorageException)
                {
                    _logger.LogError($"Response {HttpStatusCode.NotFound} was sent");
                    responseString = Serialize($"No data found to show");
                    response.ReturnNotFound(responseString);
                    continue;
                }
                catch (NoEnoughItemsInStorageException)
                {
                    _logger.LogError($"Response {HttpStatusCode.NotFound} was sent");
                    responseString =
                        Serialize(
                            $"No enough items found in storage, try to reduce {nameof(AppConfig.DaysToUseForMovingAverage)}");
                    response.ReturnNotFound(responseString);
                    continue;
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error while getting current exchange rate status, {e}");
                    response.ReturnInternalServerError();
                    continue;
                }

                response.ReturnResponse(responseString);
                _logger.LogInformation($"Response was sent successfully for {context.Request.RemoteEndPoint}");
            }
        }

        private static string Serialize(object value) => JsonConvert.SerializeObject(value);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping.");
            _httpListener.Close();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}