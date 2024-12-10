namespace Neoron.API.Models
{
    public class DiscordIngestionOptions
    {
        public int BatchSize { get; set; } = 100;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 1000;
    }
}
