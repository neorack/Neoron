namespace Neoron.API.Validation
{
    /// <summary>
    /// Represents the result of a message validation operation.
    /// </summary>
    public record MessageValidationResult(bool IsValid, string? ErrorMessage = null)
    {
        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        /// <returns>A successful validation result.</returns>
        public static MessageValidationResult Success() => new(true);

        /// <summary>
        /// Creates an error validation result with a specified message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>An error validation result.</returns>
        public static MessageValidationResult Error(string message) => new(false, message);
    }
}
