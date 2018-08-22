using DollarExchangeRateService.Models;
using System.Threading.Tasks;

namespace DollarExchangeRateService.Persistence
{
    internal interface IExchangeRateStorage
    {
        Task SaveAsync(ExchangeRateInfo value);
        Task<ExchangeRateResponse> GetCurrentStatusAsync(int periodsForMovingAverage);
    }
}