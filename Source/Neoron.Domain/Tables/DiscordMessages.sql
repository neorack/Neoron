CREATE TABLE [dbo].[ChannelGroups]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [GuildId] BIGINT NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [CreatedAt] DATETIMEOFFSET NOT NULL DEFAULT(SYSDATETIMEOFFSET()),
    [LastActiveAt] DATETIMEOFFSET NOT NULL DEFAULT(SYSDATETIMEOFFSET())
);
GO

CREATE NONCLUSTERED INDEX [IX_ChannelGroups_GuildId] ON [dbo].[ChannelGroups]([GuildId]);
GO

CREATE TABLE [dbo].[DiscordMessages]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [MessageId] BIGINT NOT NULL,
    CONSTRAINT [UQ_DiscordMessages_MessageId] UNIQUE ([MessageId]),
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
    CONSTRAINT [UQ_DiscordMessages_GuildChannel] UNIQUE ([GuildId], [ChannelId], [MessageId]),
    CONSTRAINT [FK_DiscordMessages_ReplyTo] FOREIGN KEY ([ReplyToMessageId]) 
        REFERENCES [dbo].[DiscordMessages]([MessageId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DiscordMessages_Thread] FOREIGN KEY ([ThreadId]) 
        REFERENCES [dbo].[DiscordMessages]([MessageId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DiscordMessages_Group] FOREIGN KEY ([GroupId])
        REFERENCES [dbo].[ChannelGroups]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CK_DiscordMessages_ThreadId] CHECK ([ThreadId] != [MessageId])
);
GO

CREATE NONCLUSTERED INDEX [IX_DiscordMessages_MessageId] ON [dbo].[DiscordMessages]([MessageId]);
GO
CREATE NONCLUSTERED INDEX [IX_DiscordMessages_ChannelId] ON [dbo].[DiscordMessages]([ChannelId]);
GO
CREATE NONCLUSTERED INDEX [IX_DiscordMessages_GuildId] ON [dbo].[DiscordMessages]([GuildId]);
GO
CREATE NONCLUSTERED INDEX [IX_DiscordMessages_AuthorId] ON [dbo].[DiscordMessages]([AuthorId]);
GO
CREATE NONCLUSTERED INDEX [IX_DiscordMessages_GroupId] ON [dbo].[DiscordMessages]([GroupId]);
GO
