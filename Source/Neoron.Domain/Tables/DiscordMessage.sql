CREATE TABLE [dbo].[ChannelGroups]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [GuildId] BIGINT NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [CreatedAt] DATETIMEOFFSET NOT NULL DEFAULT(SYSDATETIMEOFFSET()),
    [LastActiveAt] DATETIMEOFFSET NOT NULL DEFAULT(SYSDATETIMEOFFSET()),
    CONSTRAINT [UQ_ChannelGroups_GuildName] UNIQUE ([GuildId], [Name])
)
GO

CREATE NONCLUSTERED INDEX [IX_ChannelGroups_GuildId] ON [dbo].[ChannelGroups]([GuildId])
GO

CREATE TABLE [dbo].[DiscordMessage]
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
    [Version] ROWVERSION NOT NULL,
    [LastSyncedAt] DATETIMEOFFSET NOT NULL DEFAULT(SYSDATETIMEOFFSET()),
    [GroupId] BIGINT NULL,
    CONSTRAINT [FK_DiscordMessage_ReplyTo] FOREIGN KEY ([ReplyToMessageId]) 
        REFERENCES [dbo].[DiscordMessage]([MessageId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DiscordMessage_Thread] FOREIGN KEY ([ThreadId]) 
        REFERENCES [dbo].[DiscordMessage]([MessageId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DiscordMessage_Group] FOREIGN KEY ([GroupId])
        REFERENCES [dbo].[ChannelGroups]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CK_DiscordMessage_ThreadId] CHECK ([ThreadId] != [MessageId]),
    CONSTRAINT [CK_DiscordMessage_ReplyTo] CHECK ([ReplyToMessageId] != [MessageId])
)
GO

CREATE NONCLUSTERED INDEX [IX_DiscordMessage_ChannelId] 
ON [dbo].[DiscordMessage]([ChannelId])
INCLUDE ([MessageId], [Content], [CreatedAt])
WHERE [IsDeleted] = 0
GO

CREATE NONCLUSTERED INDEX [IX_DiscordMessage_GuildId] 
ON [dbo].[DiscordMessage]([GuildId])
INCLUDE ([MessageId], [ChannelId], [CreatedAt])
WHERE [IsDeleted] = 0
GO

CREATE NONCLUSTERED INDEX [IX_DiscordMessage_AuthorId] 
ON [dbo].[DiscordMessage]([AuthorId])
INCLUDE ([MessageId], [Content], [CreatedAt])
WHERE [IsDeleted] = 0
GO

CREATE NONCLUSTERED INDEX [IX_DiscordMessage_GroupId] 
ON [dbo].[DiscordMessage]([GroupId])
INCLUDE ([MessageId], [Content], [CreatedAt])
WHERE [IsDeleted] = 0
GO
