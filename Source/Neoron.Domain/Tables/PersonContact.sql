CREATE TABLE [dbo].[PersonContact]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [ContactTypeId] TINYINT NOT NULL,
    CONSTRAINT [FK_PersonContact_ContactType] FOREIGN KEY ([ContactTypeId]) REFERENCES [dbo].[RefContactType]([Id]),
    [Value] NVARCHAR(1000) NOT NULL,                    -- Reasonable max length for contact info
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [IsVerified] BIT NOT NULL DEFAULT 0,
    [VerifiedAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [UQ_PersonContact_Primary] UNIQUE ([PersonId], [ContactTypeId], [IsPrimary]) 
        WHERE [IsPrimary] = 1,
    CONSTRAINT [FK_PersonContact_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id])
)
GO
