CREATE TABLE [dbo].[PersonInfluence]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [MetricType] NVARCHAR(50) NOT NULL CHECK ([MetricType] IN ('NetworkSize', 'MessageReach', 'IdeologicalInfluence', 'GroupInfluence', 'Overall')),
    [Score] DECIMAL(10,2) NOT NULL,
    [CalculatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ValidUntil] DATETIME2 NULL,
    CONSTRAINT [FK_PersonInfluence_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id])
)
GO
