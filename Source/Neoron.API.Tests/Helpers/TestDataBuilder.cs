using Neoron.API.Models;

namespace Neoron.API.Tests.Helpers;

public static class TestDataBuilder
{
    public static DiscordMessage CreateTestMessage(
        long messageId = 1,
        string content = "Test message",
        long authorId = 1,
        long channelId = 1,
        long guildId = 1,
        int messageType = 0,
        DateTimeOffset? createdAt = null)
    {
        return new DiscordMessage
        {
            MessageId = messageId,
            ChannelId = channelId,
            GuildId = guildId,
            AuthorId = authorId,
            Content = content,
            MessageType = messageType,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow
        };
    }

    public static IEnumerable<DiscordMessage> CreateTestMessages(int count)
    {
        return Enumerable.Range(1, count).Select(i => CreateTestMessage(
            messageId: i,
            content: $"Test message {i}",
            authorId: 1));
    }

    public static DiscordMessage CreateThreadMessage(
        long parentMessageId,
        long messageId = 1,
        string content = "Thread reply")
    {
        var message = CreateTestMessage(messageId, content);
        message.ParentMessageId = parentMessageId;
        return message;
    }
}
