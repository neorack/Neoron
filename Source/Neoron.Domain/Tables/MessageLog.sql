CREATE TABLE [dbo].[MessageLog]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [SenderId] UNIQUEIDENTIFIER NOT NULL,
    [ReceiverId] UNIQUEIDENTIFIER NULL,
    [GroupId] UNIQUEIDENTIFIER NULL,
    [MessageType] NVARCHAR(50) NOT NULL CHECK ([MessageType] IN ('Direct', 'Group', 'System', 'Broadcast')),
    [Content] NVARCHAR(MAX) NOT NULL,
    [ContentType] NVARCHAR(50) NOT NULL CHECK ([ContentType] IN ('Text', 'HTML', 'JSON', 'Markdown')),
    [SentAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [DeliveredAt] DATETIME2 NULL,
    [ReadAt] DATETIME2 NULL,
    [Status] NVARCHAR(50) NOT NULL CHECK ([Status] IN ('Sent', 'Delivered', 'Read', 'Failed')),
    CONSTRAINT [FK_MessageLog_Sender] FOREIGN KEY ([SenderId]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_MessageLog_Receiver] FOREIGN KEY ([ReceiverId]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_MessageLog_Group] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[UserGroup]([Id]) ON DELETE CASCADE,
    CONSTRAINT [CK_MessageLog_Recipient] CHECK (
        ([ReceiverId] IS NULL AND [GroupId] IS NOT NULL) OR
        ([ReceiverId] IS NOT NULL AND [GroupId] IS NULL)
    )
)
GO

CREATE INDEX [IX_MessageLog_SenderId] ON [dbo].[MessageLog] ([SenderId])
GO

CREATE INDEX [IX_MessageLog_ReceiverId] ON [dbo].[MessageLog] ([ReceiverId]) WHERE [ReceiverId] IS NOT NULL
GO

CREATE INDEX [IX_MessageLog_GroupId] ON [dbo].[MessageLog] ([GroupId]) WHERE [GroupId] IS NOT NULL
GO

CREATE INDEX [IX_MessageLog_SentAt] ON [dbo].[MessageLog] ([SentAt])
GO
