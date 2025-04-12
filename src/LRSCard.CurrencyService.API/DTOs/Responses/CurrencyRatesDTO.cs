namespace LRSCard.CurrencyService.API.DTOs.Responses
{
    public class CurrencyRatesDTO
    {
        public float? Amount { get; set; }
        public string? BaseCurrency { get; set; }
        public DateTime? Date { get; set; }
        public Dictionary<string, float>? TargetCurrencies { get; set; }
    }
}
