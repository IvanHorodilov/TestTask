using DollarExchangeRateService.HostedServices;
using DollarExchangeRateService.Persistence;
using DollarExchangeRateService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DollarExchangeRateService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.Configure<AppConfig>(hostContext.Configuration.GetSection("AppConfig"));
                    services.AddSingleton<IExchangeRateStorage, ExchangeRateInfoStorage>();
                    services.AddSingleton<IExchangeRateService, ExchangeRateService>();
                    services.AddSingleton<IHostedService, ExchangeRateCollectorService>();
                    services.AddSingleton<IHostedService, HttpListenerService>();
                })
                .ConfigureLogging((hostingContext, logging) => {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

            await builder.RunConsoleAsync().ConfigureAwait(false);
        }
    }
}
