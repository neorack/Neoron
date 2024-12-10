using System.Text.RegularExpressions;

namespace Neoron.API.Validation
{
    /// <summary>
    /// Provides methods for validating message content.
    /// </summary>
    public static partial class MessageContentValidator
    {
        private const int MaxUrls = 5;
        private const int MaxMentions = 10;
        private const int MaxLength = 2000;
        private static readonly Regex UrlRegex = GetUrlRegex();

        /// <summary>
        /// Validates the content of a message.
        /// </summary>
        /// <param name="content">The message content to validate.</param>
        /// <returns>A validation result indicating success or failure.</returns>
        public static MessageValidationResult ValidateContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return MessageValidationResult.Error("Content cannot be empty");
            }

            if (content.Length > MaxLength)
            {
                return MessageValidationResult.Error($"Content exceeds maximum length of {MaxLength}");
            }

            var urlCount = UrlRegex.Matches(content).Count;
            if (urlCount > MaxUrls)
            {
                return MessageValidationResult.Error($"Too many URLs (max {MaxUrls})");
            }

            var mentionCount = Regex.Matches(content, @"<@!?\d+>").Count;
            return mentionCount > MaxMentions
                ? MessageValidationResult.Error($"Too many mentions (max {MaxMentions})")
                : MessageValidationResult.Success();
        }

        [GeneratedRegex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
        private static partial Regex GetUrlRegex();
    }
}
