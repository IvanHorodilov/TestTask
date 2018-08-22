using DollarExchangeRateService.Extensions;
using DollarExchangeRateService.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DollarExchangeRateService.Persistence
{
    internal class ExchangeRateInfoStorage : IExchangeRateStorage
    {
        private readonly IOptions<AppConfig> _appConfig;
        private readonly List<ExchangeRateInfo> _rateInfo;

        public ExchangeRateInfoStorage(IOptions<AppConfig> appConfig)
        {
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _rateInfo = new List<ExchangeRateInfo>();
            GenerateTestData();
        }

        public Task SaveAsync(ExchangeRateInfo value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            _rateInfo.Add(value);
            return Task.CompletedTask;
        }

        public Task<ExchangeRateResponse> GetCurrentStatusAsync(int periodsForMovingAverage)
        {
            if (!_rateInfo.Any())
                throw new NoDataFoundInStorageException();

            var averageByDays = GetAverageGroupedByDays(periodsForMovingAverage).ToList();
            if (averageByDays.Count < periodsForMovingAverage)
                throw new NoEnoughItemsInStorageException();

            var movingAverage = averageByDays.Select(a => a.Value).ToList()
                .Average();
            var currentDay = averageByDays.FirstOrDefault();
            if (currentDay == null)
                throw new NoDataFoundInStorageException();

            var status = GetExchangeRateStatus(currentDay.Value, movingAverage);
            var lastEntry = GetCurrentEntry();
            return Task.FromResult(new ExchangeRateResponse
            {
                Date = lastEntry.Date,
                Value = lastEntry.Value,
                Status = status
            });
        }

        private ExchangeRateInfo GetCurrentEntry() => _rateInfo.OrderByDescending(r => r.Date).FirstOrDefault();

        private IEnumerable<ExchangeRateInfo> GetAverageGroupedByDays(int periods)
        {
            return (_rateInfo.Where(r => DateInAnalyzePeriod(r.Date, periods))
                .GroupBy(exchangeRateInfo => exchangeRateInfo.Date.Day())
                .Select(g => new ExchangeRateInfo {Date = g.Key, Value = g.Sum(i => i.Value) / g.Count()})
                .OrderByDescending(i => i.Date));
        }

        private bool DateInAnalyzePeriod(DateTime date, int periods) =>
            date >= DateTime.Now.AddDays(-periods);

        private ExchangeRateStatusType GetExchangeRateStatus(decimal averageForDay, decimal movingAverage)
        {
            var diffInPercent = (averageForDay / movingAverage) * 100 - 100;
            if (diffInPercent == 0)
                return ExchangeRateStatusType.WithoutChanges;

            if (diffInPercent > 0)
            {
                return diffInPercent > (decimal) _appConfig.Value.PercentageForHighGrowth
                    ? ExchangeRateStatusType.HighGrowth
                    : ExchangeRateStatusType.Growth;
            }

            return diffInPercent < -(decimal) _appConfig.Value.PercentageForHighFall
                ? ExchangeRateStatusType.HighFall
                : ExchangeRateStatusType.Fall;
        }

        private void GenerateTestData()
        {
            var rnd = new Random();
            const int days = 10;
            const int timesOnDay = 10;
            var yesterday = DateTime.Now.AddDays(-1);

            for (var i = 0; i < days; i++)
            for (var j = 0; j < timesOnDay; j++)
                _rateInfo.Add(new ExchangeRateInfo
                {
                    Date = yesterday.AddDays(-i).AddHours(-j),
                    Value = rnd.NextDecimal(30, 20)
                });
        }
    }
}