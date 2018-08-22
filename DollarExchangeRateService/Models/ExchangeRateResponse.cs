using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace DollarExchangeRateService.Models
{
    internal class CustomDateTimeConverter : IsoDateTimeConverter
    {
        public CustomDateTimeConverter()
        {
            DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        }
    }

    internal class ExchangeRateResponse
    {
        private decimal _value;

        [JsonProperty("last_exchange_rate")]
        public decimal Value
        {
            get => Math.Round(_value, 2);
            set => _value = value;
        }

        //didn`t add transformation from UTC and vice versa
        [JsonConverter(typeof(CustomDateTimeConverter))]
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("status")]
        public ExchangeRateStatusType Status { get; set; }
    }
}