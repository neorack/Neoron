using Neoron.API.Interfaces;
using Neoron.API.Models;
using Neoron.API.Logging;
using Neoron.API.Middleware;
using Microsoft.Extensions.Options;

namespace Neoron.API.Services
{
    public class DiscordLogIngestionService : IDiscordLogIngestionService
    {
        private readonly IDiscordMessageRepository _repository;
        private readonly ITokenBucket _tokenBucket;
        private readonly ILogger<DiscordLogIngestionService> _logger;
        private readonly int _batchSize;

        public DiscordLogIngestionService(
            IDiscordMessageRepository repository,
            ITokenBucket tokenBucket,
            ILogger<DiscordLogIngestionService> logger,
            IOptions<RateLimitingOptions> options)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _tokenBucket = tokenBucket ?? throw new ArgumentNullException(nameof(tokenBucket));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _batchSize = options?.Value?.BatchSize ?? 100;
        }

        public async Task<int> IngestMessagesAsync(IEnumerable<DiscordMessage> messages, CancellationToken cancellationToken = default)
        {
            var messagesList = messages.ToList();
            var processedCount = 0;

            foreach (var batch in messagesList.Chunk(_batchSize))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (!_tokenBucket.ConsumeToken())
                {
                    _logger.LogWarning("Rate limit reached, waiting before processing next batch");
                    await Task.Delay(1000, cancellationToken); // Wait 1 second before retry
                    continue;
                }

                try
                {
                    LogMessages.LogAddingRange(_logger);
                    var result = await _repository.AddRangeAsync(batch);
                    processedCount += result;
                    LogMessages.LogAddedRange(_logger);
                }
                catch (Exception ex)
                {
                    LogMessages.LogAddRangeError(_logger, ex);
                    throw;
                }
            }

            return processedCount;
        }

        public bool IsRateLimited() => !_tokenBucket.ConsumeToken();
    }
}
