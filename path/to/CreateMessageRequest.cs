using System.ComponentModel.DataAnnotations;

public class CreateMessageRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Content { get; set; }

    // Add other fields and validation attributes
}
