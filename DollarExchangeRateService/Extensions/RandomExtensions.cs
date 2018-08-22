using System;

namespace DollarExchangeRateService.Extensions
{
    internal static class RandomExtensions
    {
        public static decimal NextDecimal(this Random random, int minimum, int maximum)
            //i know about the losses but it`s for testing purposes
            => (decimal)random.NextDouble() * (maximum - minimum) + minimum;
    }
}