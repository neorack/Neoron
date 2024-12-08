CREATE TABLE [dbo].[PersonContact]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [ContactType] NVARCHAR(50) NOT NULL CHECK ([ContactType] IN ('Email', 'Phone', 'Address', 'Social', 'Other')),
    [Value] NVARCHAR(1000) NOT NULL,                    -- Reasonable max length for contact info
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [IsVerified] BIT NOT NULL DEFAULT 0,
    [VerifiedAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [UQ_PersonContact_Primary] UNIQUE ([PersonId], [ContactType], [IsPrimary]) 
        WHERE [IsPrimary] = 1,
    CONSTRAINT [FK_PersonContact_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id])
)
GO
