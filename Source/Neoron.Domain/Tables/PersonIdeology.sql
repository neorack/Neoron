CREATE TABLE [dbo].[PersonIdeology]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [IdeologyId] UNIQUEIDENTIFIER NOT NULL,
    [Strength] TINYINT NOT NULL CHECK ([Strength] BETWEEN 1 AND 100),
    [StartDate] DATE NOT NULL DEFAULT GETUTCDATE(),
    [EndDate] DATE NULL CHECK ([EndDate] IS NULL OR [EndDate] >= [StartDate]),
    [IsPublic] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_PersonIdeology_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_PersonIdeology_Ideology] FOREIGN KEY ([IdeologyId]) REFERENCES [dbo].[Ideology]([Id])
)
GO