CREATE TABLE [dbo].[MessageLog]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    CONSTRAINT [PK_MessageLog] PRIMARY KEY NONCLUSTERED ([Id]),
    [SenderId] UNIQUEIDENTIFIER NOT NULL,
    [ReceiverId] UNIQUEIDENTIFIER NULL,
    [GroupId] UNIQUEIDENTIFIER NULL,
    [MessageType] NVARCHAR(50) NOT NULL CONSTRAINT [CK_MessageLog_MessageType] CHECK ([MessageType] IN ('Direct', 'Group', 'System', 'Broadcast')),
    [Content] NVARCHAR(MAX) NOT NULL,
    [ContentType] NVARCHAR(50) NOT NULL CONSTRAINT [CK_MessageLog_ContentType] CHECK ([ContentType] IN ('Text', 'HTML', 'JSON', 'Markdown')),
    [SentAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [DeliveredAt] DATETIME2 NULL,
    [ReadAt] DATETIME2 NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Sent' CONSTRAINT [CK_MessageLog_Status] CHECK ([Status] IN ('Sent', 'Delivered', 'Read', 'Failed')),
    CONSTRAINT [FK_MessageLog_Sender] FOREIGN KEY ([SenderId]) REFERENCES [dbo].[Person]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MessageLog_Receiver] FOREIGN KEY ([ReceiverId]) REFERENCES [dbo].[Person]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MessageLog_Group] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[UserGroup]([Id]) ON DELETE CASCADE,
    CONSTRAINT [CK_MessageLog_Recipient] CHECK (
        ([ReceiverId] IS NULL AND [GroupId] IS NOT NULL) OR
        ([ReceiverId] IS NOT NULL AND [GroupId] IS NULL)
    )
)
GO

CREATE CLUSTERED INDEX [CIX_MessageLog_SentAt] ON [dbo].[MessageLog] ([SentAt])
INCLUDE ([SenderId], [ReceiverId], [GroupId], [MessageType], [Status])
WITH (DATA_COMPRESSION = PAGE)
GO

CREATE INDEX [IX_MessageLog_SenderId] ON [dbo].[MessageLog] ([SenderId])
INCLUDE ([MessageType], [SentAt], [Status])
WITH (DATA_COMPRESSION = PAGE)
GO

CREATE INDEX [IX_MessageLog_ReceiverId] ON [dbo].[MessageLog] ([ReceiverId]) 
INCLUDE ([SenderId], [MessageType], [Content], [SentAt]) 
WHERE [ReceiverId] IS NOT NULL
WITH (DATA_COMPRESSION = PAGE)
GO

CREATE INDEX [IX_MessageLog_GroupId] ON [dbo].[MessageLog] ([GroupId])
INCLUDE ([SenderId], [MessageType], [Content], [SentAt])
WHERE [GroupId] IS NOT NULL
WITH (DATA_COMPRESSION = PAGE)
GO

CREATE INDEX [IX_MessageLog_Status] ON [dbo].[MessageLog] ([Status], [SentAt])
INCLUDE ([SenderId], [ReceiverId], [GroupId], [Content])
WHERE [Status] != 'Read' AND [Status] != 'Failed'
WITH (DATA_COMPRESSION = PAGE)
GO

CREATE INDEX [IX_MessageLog_SenderReceiver] ON [dbo].[MessageLog] ([SenderId], [ReceiverId], [SentAt])
INCLUDE ([MessageType], [Content], [Status])
WHERE [ReceiverId] IS NOT NULL
WITH (DATA_COMPRESSION = PAGE)
GO
