using DollarExchangeRateService.Models;
using System.Threading.Tasks;

namespace DollarExchangeRateService.Services
{
    internal interface IExchangeRateService
    {
        Task<ExchangeRateInfo> GetLastAsync();
    }
}
