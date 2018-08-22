using System;

namespace DollarExchangeRateService.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime Day(this DateTime dateTime) =>
            new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
    }
}