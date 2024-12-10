CREATE TABLE [dbo].[ActivityLog]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [ActivityType] NVARCHAR(50) NOT NULL CHECK ([ActivityType] IN ('Login', 'Message', 'GroupJoin', 'IdeologyChange', 'RelationshipChange', 'ProfileUpdate', 'Other')),
    [Description] NVARCHAR(MAX) NULL,
    [Metadata] NVARCHAR(MAX) NULL,
    [IpAddress] NVARCHAR(45) NULL,
    [UserAgent] NVARCHAR(500) NULL,
    [OccurredAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_ActivityLog_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id])
)
GO

CREATE INDEX [IX_ActivityLog_PersonId] ON [dbo].[ActivityLog] ([PersonId])
GO

CREATE INDEX [IX_ActivityLog_ActivityType] ON [dbo].[ActivityLog] ([ActivityType])
GO

CREATE INDEX [IX_ActivityLog_OccurredAt] ON [dbo].[ActivityLog] ([OccurredAt])
GO
