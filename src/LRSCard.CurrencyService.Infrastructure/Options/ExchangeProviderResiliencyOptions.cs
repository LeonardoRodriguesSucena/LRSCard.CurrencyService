using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRSCard.CurrencyService.Infrastructure.Options
{
    public class ExchangeProviderResiliencyOptions
    {
        public int RetryCount { get; set; } = 3;
        public int InitialBackoffSeconds { get; set; } = 2;
        public int CircuitBreakerFailureThreshold { get; set; } = 2;
        public int CircuitBreakerDurationInSeconds { get; set; } = 30;
    }
}
