using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRSCard.CurrencyService.Infrastructure.ExchangeRateProviders.Frankfurter
{
    //response object from Frankfurter API
    class FrankfurterAPIResponse
    {
        public float? Amount { get; set; }
        public string? Base { get; set; }
        public DateTime? Date { get; set; }
        public Dictionary<string, float>? Rates { get; set; }
    }
}
