CREATE TABLE [dbo].[PersonTag]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [TagId] UNIQUEIDENTIFIER NOT NULL,
    [Weight] TINYINT NULL CHECK ([Weight] BETWEEN 1 AND 100),
    [Source] NVARCHAR(50) NOT NULL CHECK ([Source] IN ('Self', 'System', 'Peer', 'AI')),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [FK_PersonTag_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_PersonTag_Tag] FOREIGN KEY ([TagId]) REFERENCES [dbo].[Tag]([Id]),
    CONSTRAINT [FK_PersonTag_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Person]([Id])
)
GO
