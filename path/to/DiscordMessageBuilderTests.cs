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
        Assert.IsNull(message.Content);
        Assert.IsNull(message.EmbeddedContent);
        Assert.AreEqual(default(Guid), message.AuthorId);
        Assert.AreEqual(default(DateTimeOffset), message.CreatedAt);
        Assert.AreEqual(default(DateTimeOffset?), message.EditedAt);
        // Add more assertions to verify default values
    }

    [Test]
    public void SetProperties_ShouldApplyCorrectly()
    {
        // Arrange
        var builder = new DiscordMessageBuilder();
        var expectedContent = "Hello, World!";
        var expectedEmbeddedContent = "Embedded Content";
        var expectedAuthorId = Guid.NewGuid();

        // Act
        builder.SetContent(expectedContent);
        builder.SetEmbeddedContent(expectedEmbeddedContent);
        builder.SetAuthorId(expectedAuthorId);
        var message = builder.Create();

        // Assert
        Assert.AreEqual(expectedContent, message.Content);
        Assert.AreEqual(expectedEmbeddedContent, message.EmbeddedContent);
        Assert.AreEqual(expectedAuthorId, message.AuthorId);
        // Add more assertions for other properties
    }
}
