CREATE TABLE [dbo].[PersonRelationship]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [RelatedPersonId] UNIQUEIDENTIFIER NOT NULL,
    [RelationType] NVARCHAR(50) NOT NULL,               -- Friend, Family, Colleague, etc.
    [Strength] TINYINT NULL CHECK ([Strength] BETWEEN 1 AND 100), -- Optional relationship strength 1-100
    [StartDate] DATE NULL,
    [EndDate] DATE NULL CHECK ([EndDate] IS NULL OR [EndDate] >= [StartDate]),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_PersonRelationship_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_PersonRelationship_RelatedPerson] FOREIGN KEY ([RelatedPersonId]) REFERENCES [dbo].[Person]([Id])
)
GO

-- Indexes
CREATE INDEX [IX_PersonRelationship_PersonId] ON [dbo].[PersonRelationship] ([PersonId])
GO

CREATE INDEX [IX_PersonRelationship_RelatedPersonId] ON [dbo].[PersonRelationship] ([RelatedPersonId])
GO
