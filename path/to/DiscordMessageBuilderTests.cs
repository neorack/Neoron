using NUnit.Framework;

[TestFixture]
public class DiscordMessageBuilderTests
{
    [Test]
    public void Create_ShouldInitializeWithDefaultValues()
    {
        // Arrange
        var builder = new DiscordMessageBuilder();

        // Act
        var message = builder.Create();

        // Assert
        Assert.IsNotNull(message);
        // Add more assertions to verify default values
    }

    [Test]
    public void SetProperties_ShouldApplyCorrectly()
    {
        // Arrange
        var builder = new DiscordMessageBuilder();
        var expectedContent = "Hello, World!";

        // Act
        builder.SetContent(expectedContent);
        var message = builder.Create();

        // Assert
        Assert.AreEqual(expectedContent, message.Content);
        // Add more assertions for other properties
    }
}
