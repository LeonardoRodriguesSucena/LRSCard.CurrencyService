using System.Security.Claims;
using LRSCard.CurrencyService.Application.Common;

namespace LRSCard.CurrencyService.API.Context
{
    public class RequestContext : IRequestContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? CorrelationId =>
            _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();

        public string? UserId =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        public string? Sub =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
