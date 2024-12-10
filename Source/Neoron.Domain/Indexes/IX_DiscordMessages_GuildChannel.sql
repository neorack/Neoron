CREATE NONCLUSTERED INDEX [IX_DiscordMessage_GuildChannel]
ON [dbo].[DiscordMessage] ([GuildId], [ChannelId])
INCLUDE ([MessageId], [AuthorId], [Content], [CreatedAt])
WHERE [IsDeleted] = 0;
GO
