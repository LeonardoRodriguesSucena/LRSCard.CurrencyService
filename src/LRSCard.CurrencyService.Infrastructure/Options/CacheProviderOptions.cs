using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRSCard.CurrencyService.Infrastructure.Options
{
    public class CacheProviderOptions
    {
        public int CacheExpirationInDays { get; set; } = 90;
    }
}
