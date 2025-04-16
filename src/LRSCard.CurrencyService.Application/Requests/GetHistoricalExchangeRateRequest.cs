using LRSCard.CurrencyService.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRSCard.CurrencyService.Application.Requests
{
    public class GetHistoricalExchangeRateRequest
    {
        public float? Amount { get; set; } = null;
        public DateTime InitialDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? BaseCurrency { get; set; } = null;
        public List<string>? Symbols { get; set; } = null;
        public Pagination Pagination { get; set; } = new Pagination();

        //preparing for future aditional providers
        public CurrencyProviderType CurrencyProvider { get; set; } = CurrencyProviderType.Frankfurter;
    }

}
