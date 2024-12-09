using FluentAssertions;
using Neoron.API.Validation;
using Xunit;

namespace Neoron.API.Tests.Security;

public class InputSanitizerTests
{
    [Theory]
    [InlineData("<script>alert('xss')</script>", "alert('xss')")]
    [InlineData("javascript:alert('xss')", "alert('xss')")]
    [InlineData("<img src=x onerror=alert('xss')>", "<img src=x>")]
    public void SanitizeContent_RemovesScriptContent(string input, string expected)
    {
        // Act
        var result = InputSanitizer.SanitizeContent(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("SELECT * FROM Users", true)]
    [InlineData("Normal message", false)]
    [InlineData("DROP TABLE Messages", true)]
    public void ContainsSqlInjection_DetectsCorrectly(string input, bool expectedResult)
    {
        // Act
        var result = InputSanitizer.ContainsSqlInjection(input);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("Hello 👋", true)]
    [InlineData("Test\0message", false)]
    [InlineData("Normal message", true)]
    public void IsValidContent_ValidatesCorrectly(string input, bool expectedResult)
    {
        // Act
        var result = InputSanitizer.IsValidContent(input);

        // Assert
        result.Should().Be(expectedResult);
    }
}
