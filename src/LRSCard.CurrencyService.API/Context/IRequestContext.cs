using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRSCard.CurrencyService.Application.Common
{
    public interface IRequestContext
    {
        string? CorrelationId { get; }
        string? UserId { get; }
        string? Sub { get; }
    }
}
