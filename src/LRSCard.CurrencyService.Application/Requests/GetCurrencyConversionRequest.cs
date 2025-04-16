using LRSCard.CurrencyService.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRSCard.CurrencyService.Application.Requests
{
    public class GetCurrencyConversionRequest
    {
        public float Amount { get; set; }
        public string BaseCurrency { get; set; }
        public List<string> Symbols { get; set; }

        //preparing for future aditional providers
        public CurrencyProviderType CurrencyProvider { get; set; } = CurrencyProviderType.Frankfurter;

    }

}
