using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRSCard.CurrencyService.Application.Options
{
    public class CurrencyRulesOptions
    {
        public List<string> ValidCurrencyCodes { get; set; } = new();
        public List<string> BlockedCurrencyCodes { get; set; } = new();
    }
}
