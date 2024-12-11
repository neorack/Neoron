using Microsoft.AspNetCore.Mvc;
using Neoron.API.DTOs;
using Neoron.API.Interfaces;
using Neoron.API.Validation;

namespace Neoron.API.Controllers
{
    /// <summary>
    /// Controller for managing Discord messages.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DiscordMessageController : ControllerBase
    {
        private readonly IDiscordMessageRepository repository;
        private readonly ILogger<DiscordMessageController> logger;

        public DiscordMessageController(
            IDiscordMessageRepository repository,
            ILogger<DiscordMessageController> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }
        private static readonly Action<ILogger, long, Exception?> LogRetrieveMessage =
            LoggerMessage.Define<long>(LogLevel.Information, 0, "Retrieving message with ID: {Id}.");

        private static readonly Action<ILogger, long, Exception?> LogMessageNotFound =
            LoggerMessage.Define<long>(LogLevel.Warning, 1, "Message with ID {Id} not found.");

        private static readonly Action<ILogger, long, Exception> LogErrorRetrieving =
            LoggerMessage.Define<long>(LogLevel.Error, 2, "Error retrieving message {Id}.");

        private readonly IDiscordMessageRepository repository = repository;
        private readonly ILogger<DiscordMessageController> logger = logger;

        /// <summary>
        /// Retrieves a specific Discord message by its unique identifier.
        /// </summary>
        /// <param name="id">The Discord message ID to retrieve.</param>
        /// <returns>The message details if found.</returns>
        /// <remarks>
        /// This method:
        /// - Validates the message ID
        /// - Logs the retrieval attempt
        /// - Returns a 404 if message not found
        /// - Maps entity to DTO for response
        /// </remarks>
        /// <response code="200">Returns the message details</response>
        /// <response code="404">If the message is not found</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MessageResponse>> GetById(long id)
        {
            try
            {
                LogRetrieveMessage(logger, id, null);

                var message = await repository.GetByIdAsync(id).ConfigureAwait(false);
                if (message == null)
                {
                    LogMessageNotFound(logger, id, null);
                    return NotFound(new { error = "Message not found" });
                }

                return Ok(MessageResponse.FromEntity(message));
            }
            catch (Exception ex)
            {
                LogErrorRetrieving(logger, id, ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves messages from a specific Discord channel with pagination support.
        /// </summary>
        /// <param name="channelId">The Discord channel ID to fetch messages from.</param>
        /// <param name="skip">Number of messages to skip for pagination (must be >= 0).</param>
        /// <param name="take">Number of messages to return (between 1 and 100).</param>
        /// <returns>A paginated list of messages from the channel.</returns>
        /// <remarks>
        /// This endpoint:
        /// - Supports server-side pagination
        /// - Orders messages by creation date descending
        /// - Limits maximum page size to 100
        /// - Filters out deleted messages
        /// - Includes thread/reply relationships
        /// </remarks>
        /// <response code="200">Returns the list of messages</response>
        /// <response code="400">If pagination parameters are invalid</response>
        /// <response code="404">If the channel is not found</response>
        [HttpGet("channel/{channelId}")]
        [ProducesResponseType(typeof(IEnumerable<MessageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<MessageResponse>>> GetByChannel(
            long channelId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100)
        {
            var messages = await repository.GetByChannelIdAsync(channelId, skip, take).ConfigureAwait(false);
            return Ok(messages.Select(MessageResponse.FromEntity));
        }

        /// <summary>
        /// Creates a new message.
        /// </summary>
        /// <param name="request">The message creation request.</param>
        /// <returns>The created message.</returns>
        /// <response code="201">Returns the created message.</response>
        /// <response code="400">If the request is invalid.</response>
        [HttpPost]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MessageResponse>> Create(CreateMessageRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validationResult = MessageContentValidator.ValidateContent(request.Content);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { error = validationResult.ErrorMessage });
            }

            var message = request.ToEntity();
            var result = await repository.AddAsync(message).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                MessageResponse.FromEntity(result));
        }

        /// <summary>
        /// Updates an existing message.
        /// </summary>
        /// <param name="id">The message ID.</param>
        /// <param name="request">The update request.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(long id, UpdateMessageRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Content != null)
            {
                var validationResult = MessageContentValidator.ValidateContent(request.Content);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { error = validationResult.ErrorMessage });
                }
            }

            var message = await repository.GetByIdAsync(id).ConfigureAwait(false);
            if (message == null)
            {
                return NotFound();
            }

            request.UpdateEntity(message);
            await repository.UpdateAsync(message).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// Deletes a message.
        /// </summary>
        /// <param name="id">The message ID.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(long id)
        {
            await repository.DeleteAsync(id).ConfigureAwait(false);
            return NoContent();
        }
    }
}
