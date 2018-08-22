using DollarExchangeRateService.Extensions;
using DollarExchangeRateService.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DollarExchangeRateService.Services
{
    internal class ExchangeRateService : IExchangeRateService
    {
        private readonly Random _rnd = new Random();

        public Task<ExchangeRateInfo> GetLastAsync()
        {
            if (_rnd.Next(1, 4) == 1)
                throw new WebException();

            return Task.FromResult(new ExchangeRateInfo {Date = DateTime.Now, Value = _rnd.NextDecimal(30, 20)});
        }
    }
}