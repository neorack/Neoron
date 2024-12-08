CREATE NONCLUSTERED INDEX [IX_DiscordMessages_GuildChannel]
ON [dbo].[DiscordMessages] ([GuildId], [ChannelId])
INCLUDE ([MessageId], [AuthorId], [Content], [CreatedAt]);
