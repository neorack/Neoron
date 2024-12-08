using System.Text.RegularExpressions;

namespace Neoron.API.Validation;

public static partial class MessageContentValidator
{
    private static readonly Regex UrlRegex = GetUrlRegex();
    private const int MaxUrls = 5;
    private const int MaxMentions = 10;
    private const int MaxLength = 2000;

    /// <summary>
    /// Validates the content of a message.
    /// </summary>
    /// <param name="content">The message content to validate.</param>
    /// <returns>A validation result indicating success or failure.</returns>
    public static ValidationResult ValidateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return ValidationResult.Error("Content cannot be empty");

        if (content.Length > MaxLength)
            return ValidationResult.Error($"Content exceeds maximum length of {MaxLength}");

        var urlCount = UrlRegex.Matches(content).Count;
        if (urlCount > MaxUrls)
            return ValidationResult.Error($"Too many URLs (max {MaxUrls})");

        var mentionCount = Regex.Matches(content, @"<@!?\d+>").Count;
        if (mentionCount > MaxMentions)
            return ValidationResult.Error($"Too many mentions (max {MaxMentions})");

        return ValidationResult.Success();
    }

    [GeneratedRegex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    private static partial Regex GetUrlRegex();
}

public record ValidationResult(bool IsValid, string? Error = null)
{
    public static ValidationResult Success() => new(true);
    public static ValidationResult Error(string message) => new(false, message);
}
