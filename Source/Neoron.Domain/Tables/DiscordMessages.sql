CREATE TABLE [dbo].[DiscordMessages]
(
    [MessageId] BIGINT NOT NULL PRIMARY KEY,
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
    CONSTRAINT [FK_DiscordMessages_ReplyTo] FOREIGN KEY ([ReplyToMessageId]) REFERENCES [dbo].[DiscordMessages]([MessageId]),
    CONSTRAINT [FK_DiscordMessages_Thread] FOREIGN KEY ([ThreadId]) REFERENCES [dbo].[DiscordMessages]([MessageId])
);
