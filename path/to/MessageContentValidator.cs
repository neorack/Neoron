public class MessageContentValidator
{
    public MessageValidationResult Validate(string content)
    {
        var result = new MessageValidationResult();

        if (content.Length > 200)
        {
            result.Errors.Add("Content exceeds maximum length.");
        }

        // Add more validation rules

        return result;
    }
}
