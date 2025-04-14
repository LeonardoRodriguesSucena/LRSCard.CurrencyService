namespace LRSCard.CurrencyService.API.Validations
{
    public static class CurrencyCodeValidator
    {
        private static HashSet<string> _currencyCodes = new();
        private static HashSet<string> _blockedCurrencyCodes = new();

        public static void Initialize(IConfiguration configuration)
        {
            var section = configuration.GetSection("CurrencyRules");
            var allowed = section.GetSection("ValidCurrencyCodes").Get<List<string>>();
            var blocked = section.GetSection("BlockedCurrencyCodes").Get<List<string>>();

            _currencyCodes = new HashSet<string>(allowed ?? [], StringComparer.OrdinalIgnoreCase);
            _blockedCurrencyCodes = new HashSet<string>(blocked ?? [], StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsValidCurrencyCode(string currencyCode) => _currencyCodes.Contains(currencyCode);

        public static bool IsBlocked(string currencyCode) => _blockedCurrencyCodes.Contains(currencyCode);
    }
}
