CREATE TABLE [dbo].[UserGroup]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Type] NVARCHAR(50) NOT NULL CHECK ([Type] IN ('Social', 'Professional', 'Interest', 'Project', 'Other')),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] UNIQUEIDENTIFIER NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [FK_Group_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_Group_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[Person]([Id])
)
GO
