using LRSCard.CurrencyService.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRSCard.CurrencyService.Application.Requests
{
    public class GetExchangeRateRequest
    {
        public float? Amount { get; set; } = null;
        public DateTime? Date { get; set; } = null;
        public string? BaseCurrency { get; set; } = null;
        public List<string>? Symbols { get; set; } = null;        
    }

}
