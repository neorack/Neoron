CREATE TABLE [dbo].[Tag]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [Name] NVARCHAR(100) NOT NULL UNIQUE,
    [Category] NVARCHAR(50) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [FK_Tag_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_Tag_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[Person]([Id])
)
GO