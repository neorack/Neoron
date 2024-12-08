CREATE TABLE [dbo].[DiscordMessage]
(
    [MessageId] BIGINT NOT NULL,
    [ChannelId] BIGINT NOT NULL,
    [GuildId] BIGINT NOT NULL,
    [AuthorId] BIGINT NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [EmbeddedContent] NVARCHAR(MAX) NULL,
    [MessageType] TINYINT NOT NULL DEFAULT 0, -- 0=Regular, 1=System, etc.
    [CreatedAt] DATETIMEOFFSET NOT NULL,
    [EditedAt] DATETIMEOFFSET NULL,
    [DeletedAt] DATETIMEOFFSET NULL,
    [ReplyToMessageId] BIGINT NULL,
    [ThreadId] BIGINT NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_DiscordMessage] PRIMARY KEY CLUSTERED ([MessageId]),
    CONSTRAINT [FK_DiscordMessage_ReplyTo] FOREIGN KEY ([ReplyToMessageId]) 
        REFERENCES [dbo].[DiscordMessage] ([MessageId]),
    CONSTRAINT [FK_DiscordMessage_Thread] FOREIGN KEY ([ThreadId])
        REFERENCES [dbo].[DiscordMessage] ([MessageId])
)
GO

CREATE INDEX [IX_DiscordMessage_ChannelId] ON [dbo].[DiscordMessage] ([ChannelId])
GO

CREATE INDEX [IX_DiscordMessage_GuildId] ON [dbo].[DiscordMessage] ([GuildId])
GO

CREATE INDEX [IX_DiscordMessage_AuthorId] ON [dbo].[DiscordMessage] ([AuthorId])
GO

CREATE INDEX [IX_DiscordMessage_ReplyToMessageId] ON [dbo].[DiscordMessage] ([ReplyToMessageId])
GO

CREATE INDEX [IX_DiscordMessage_ThreadId] ON [dbo].[DiscordMessage] ([ThreadId])
GO
