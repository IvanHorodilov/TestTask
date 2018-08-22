namespace DollarExchangeRateService
{
    public class AppConfig
    {
        public int DataCollectorIntervalInHours { get; set; }
        public int FirstPauseBetweenFailuresInSec { get; set; }
        public int MaxRetryAttempts { get; set; }
        public int DaysToUseForMovingAverage { get; set; }
        public double PercentageForHighFall { get; set; }
        public double PercentageForHighGrowth { get; set; }
    }
}
