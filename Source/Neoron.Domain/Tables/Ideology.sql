CREATE TABLE [dbo].[Ideology]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Category] NVARCHAR(50) NOT NULL CHECK ([Category] IN ('Political', 'Religious', 'Philosophical', 'Social', 'Economic', 'Other')),
    [ParentId] UNIQUEIDENTIFIER NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Ideology_Parent] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Ideology]([Id])
)
GO
