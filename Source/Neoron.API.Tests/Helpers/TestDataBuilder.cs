using Neoron.API.Models;

namespace Neoron.API.Tests.Helpers;

public static class TestDataBuilder
{
    public static DiscordMessage CreateTestMessage(
        long messageId = 1,
        string content = "Test message",
        long authorId = 1)
    {
        return new DiscordMessage
        {
            MessageId = messageId,
            ChannelId = 1,
            GuildId = 1,
            AuthorId = authorId,
            Content = content,
            MessageType = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
