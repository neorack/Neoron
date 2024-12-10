CREATE TABLE [dbo].[DiscordMessages]
(
    [Id] BIGINT NOT NULL PRIMARY KEY,
    [MessageId] BIGINT NOT NULL UNIQUE,
    [ChannelId] BIGINT NOT NULL,
    [GuildId] BIGINT NOT NULL,
    [AuthorId] BIGINT NOT NULL,
    [Content] NVARCHAR(2000) NOT NULL,
    [EmbeddedContent] NVARCHAR(MAX) NULL,
    [MessageType] TINYINT NOT NULL DEFAULT(0),
    [CreatedAt] DATETIMEOFFSET NOT NULL,
    [EditedAt] DATETIMEOFFSET NULL,
    [DeletedAt] DATETIMEOFFSET NULL,
    [ReplyToMessageId] BIGINT NULL,
    [ThreadId] BIGINT NULL,
    [IsDeleted] BIT NOT NULL DEFAULT(0),
    [Version] ROWVERSION NOT NULL,
    [LastSyncedAt] DATETIMEOFFSET NOT NULL DEFAULT(SYSDATETIMEOFFSET()),
    [GroupId] BIGINT NULL,
    CONSTRAINT [FK_DiscordMessages_ReplyTo] FOREIGN KEY ([ReplyToMessageId]) 
        REFERENCES [dbo].[DiscordMessages]([Id]),
    CONSTRAINT [FK_DiscordMessages_Thread] FOREIGN KEY ([ThreadId]) 
        REFERENCES [dbo].[DiscordMessages]([Id]),
    CONSTRAINT [FK_DiscordMessages_Group] FOREIGN KEY ([GroupId])
        REFERENCES [dbo].[ChannelGroups]([Id])
);

CREATE NONCLUSTERED INDEX [IX_DiscordMessages_MessageId] ON [dbo].[DiscordMessages]([MessageId]);
CREATE NONCLUSTERED INDEX [IX_DiscordMessages_ChannelId] ON [dbo].[DiscordMessages]([ChannelId]);
CREATE NONCLUSTERED INDEX [IX_DiscordMessages_GuildId] ON [dbo].[DiscordMessages]([GuildId]);
CREATE NONCLUSTERED INDEX [IX_DiscordMessages_AuthorId] ON [dbo].[DiscordMessages]([AuthorId]);
CREATE NONCLUSTERED INDEX [IX_DiscordMessages_GroupId] ON [dbo].[DiscordMessages]([GroupId]);
