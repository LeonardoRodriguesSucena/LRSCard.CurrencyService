using Microsoft.AspNetCore.Mvc;

namespace LRSCard.CurrencyService.API.DTOs.Requests
{
    public class GetHistoricalExchangeRateRequestDTO
    {
        /// <summary>
        /// The base currency (USD, EUR,...)
        /// </summary>
        [FromQuery(Name = "baseCurrency")]
        public string BaseCurrency { get; set; }

        /// <summary>
        /// Start of the date range (format: yyyy-MM-dd)
        /// </summary>
        [FromQuery(Name = "initialDate")]
        public DateTime InitialDate { get; set; }

        /// <summary>
        /// End of the date range (format: yyyy-MM-dd)
        /// </summary>
        [FromQuery(Name = "endDate")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Page number (must be greater than 0)
        /// </summary>
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Page size (between 1 and 60)
        /// </summary>
        [FromQuery(Name = "pageSize")]
        public int PageSize { get; set; } = 10;
    }
}
