using Microsoft.AspNetCore.Mvc;
using Neoron.API.DTOs;
using Neoron.API.Interfaces;
using Neoron.API.Validation;

namespace Neoron.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DiscordMessageController : ControllerBase
    {
        private readonly IDiscordMessageRepository _repository;
        private readonly ILogger<DiscordMessageController> _logger;

        public DiscordMessageController(
            IDiscordMessageRepository repository,
            ILogger<DiscordMessageController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a message by its ID
        /// </summary>
        /// <param name="id">The Discord message ID</param>
        /// <returns>The message if found</returns>
        /// <response code="200">Returns the message</response>
        /// <response code="404">If the message is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MessageResponse>> GetById(long id)
        {
            try
            {
                _logger.LogInformation("Retrieving message with ID: {MessageId}", id);

                var message = await _repository.GetByIdAsync(id).ConfigureAwait(false);
                if (message == null)
                {
                    _logger.LogWarning("Message with ID {MessageId} not found", id);
                    return NotFound(new { error = "Message not found" });
                }

                return Ok(MessageResponse.FromEntity(message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving message {MessageId}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets messages from a specific channel
        /// </summary>
        /// <param name="channelId">The Discord channel ID</param>
        /// <param name="skip">Number of messages to skip</param>
        /// <param name="take">Number of messages to take</param>
        /// <returns>A list of messages</returns>
        [HttpGet("channel/{channelId}")]
        [ProducesResponseType(typeof(IEnumerable<MessageResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MessageResponse>>> GetByChannel(
            long channelId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100)
        {
            var messages = await _repository.GetByChannelIdAsync(channelId, skip, take).ConfigureAwait(false);
            return Ok(messages.Select(MessageResponse.FromEntity));
        }

        /// <summary>
        /// Creates a new message
        /// </summary>
        /// <param name="request">The message creation request</param>
        /// <returns>The created message</returns>
        /// <response code="201">Returns the created message</response>
        /// <response code="400">If the request is invalid</response>
        [HttpPost]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MessageResponse>> Create(CreateMessageRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var validationResult = MessageContentValidator.ValidateContent(request.Content);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { error = validationResult.ErrorMessage });
            }

            var message = request.ToEntity();
            var result = await _repository.AddAsync(message).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.MessageId },
                MessageResponse.FromEntity(result));
        }

        /// <summary>
        /// Updates an existing message
        /// </summary>
        /// <param name="id">The message ID</param>
        /// <param name="request">The update request</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(long id, UpdateMessageRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var validationResult = MessageContentValidator.ValidateContent(request.Content);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { error = validationResult.ErrorMessage });
            }

            var message = await _repository.GetByIdAsync(id).ConfigureAwait(false);
            if (message == null)
            {
                return NotFound();
            }

            request.UpdateEntity(message);
            await _repository.UpdateAsync(message).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="id">The message ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(long id)
        {
            await _repository.DeleteAsync(id).ConfigureAwait(false);
            return NoContent();
        }
    }
}
