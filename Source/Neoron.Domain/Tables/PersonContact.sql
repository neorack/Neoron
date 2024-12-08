CREATE TABLE [dbo].[PersonContact]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [ContactTypeId] TINYINT NOT NULL,
    CONSTRAINT [FK_PersonContact_ContactType] FOREIGN KEY ([ContactTypeId]) REFERENCES [dbo].[RefContactType]([Id]) ON DELETE CASCADE,
    [Value] NVARCHAR(256) NOT NULL,                     -- Adjusted length for contact info
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [IsVerified] BIT NOT NULL DEFAULT 0,
    [VerifiedAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [UQ_PersonContact_Primary] UNIQUE ([PersonId], [ContactTypeId], [IsPrimary]) 
        WHERE [IsPrimary] = 1,
    CONSTRAINT [FK_PersonContact_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id]) ON DELETE CASCADE
)
GO

CREATE NONCLUSTERED INDEX [IX_PersonContact_PersonId_ContactTypeId] ON [dbo].[PersonContact]
(
    [PersonId] ASC,
    [ContactTypeId] ASC
)
GO

CREATE TRIGGER [dbo].[TR_PersonContact_UpdatedAt] ON [dbo].[PersonContact]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[PersonContact]
    SET [UpdatedAt] = GETUTCDATE()
    FROM [dbo].[PersonContact] pc
    INNER JOIN inserted i ON pc.[Id] = i.[Id]
END
GO
