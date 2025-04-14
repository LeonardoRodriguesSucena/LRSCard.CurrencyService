namespace LRSCard.CurrencyService.API.Options
{
    public class ApiRateLimitPolicy
    {
        public int PermitLimit { get; set; }
        public int WindowSeconds { get; set; }
        public int QueueLimit { get; set; }
    }

    public class ApiRateLimitOptions
    {
        // structuring like this allows for easy addition of new policies in the future

        public ApiRateLimitPolicy Default { get; set; } = new();
        public ApiRateLimitPolicy Anonymous { get; set; } = new();

    }

}
