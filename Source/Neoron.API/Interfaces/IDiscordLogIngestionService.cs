using Neoron.API.Models;

namespace Neoron.API.Interfaces
{
    public interface IDiscordLogIngestionService
    {
        /// <summary>
        /// Ingests a batch of Discord messages with rate limiting.
        /// </summary>
        /// <param name="messages">Collection of messages to ingest</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of successfully ingested messages</returns>
        Task<int> IngestMessagesAsync(IEnumerable<DiscordMessage> messages, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the current rate limit status
        /// </summary>
        /// <returns>True if currently rate limited, false otherwise</returns>
        bool IsRateLimited();
    }
}
