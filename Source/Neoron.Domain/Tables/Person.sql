-- Core Person table with essential information
CREATE TABLE [dbo].[Person]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [ExternalId] NVARCHAR(100) NULL,                    -- For external system integration
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [MiddleName] NVARCHAR(100) NULL,
    [PreferredName] NVARCHAR(100) NULL,
    [Email] NVARCHAR(256) NOT NULL UNIQUE,
    [DateOfBirth] DATE NULL,
    [Gender] NVARCHAR(50) NULL,
    [ProfilePictureUrl] NVARCHAR(2048) NULL,
    [TimeZone] NVARCHAR(100) NULL,                      -- For global user base
    [PreferredLanguage] NCHAR(5) NULL,                  -- ISO language code
    [LastLoginAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] UNIQUEIDENTIFIER NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [DeletedAt] DATETIME2 NULL,                         -- Soft delete support
    [DeletedBy] UNIQUEIDENTIFIER NULL
)
GO

-- Contact Information table for multiple contact methods
CREATE TABLE [dbo].[PersonContact]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [ContactType] NVARCHAR(50) NOT NULL,                -- Email, Phone, Address, etc.
    [Value] NVARCHAR(MAX) NOT NULL,
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [IsVerified] BIT NOT NULL DEFAULT 0,
    [VerifiedAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_PersonContact_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id])
)
GO

-- Relationships table for person-to-person connections
CREATE TABLE [dbo].[PersonRelationship]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [RelatedPersonId] UNIQUEIDENTIFIER NOT NULL,
    [RelationType] NVARCHAR(50) NOT NULL,               -- Friend, Family, Colleague, etc.
    [Strength] TINYINT NULL,                           -- Optional relationship strength 1-100
    [StartDate] DATE NULL,
    [EndDate] DATE NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_PersonRelationship_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_PersonRelationship_RelatedPerson] FOREIGN KEY ([RelatedPersonId]) REFERENCES [dbo].[Person]([Id])
)
GO

-- Groups table
CREATE TABLE [dbo].[Group]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Type] NVARCHAR(50) NOT NULL,                      -- Social, Professional, Interest, etc.
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] UNIQUEIDENTIFIER NULL,
    [IsActive] BIT NOT NULL DEFAULT 1
)
GO

-- Person-Group memberships
CREATE TABLE [dbo].[PersonGroup]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [GroupId] UNIQUEIDENTIFIER NOT NULL,
    [Role] NVARCHAR(50) NOT NULL,                      -- Member, Admin, Moderator, etc.
    [JoinedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ExpiresAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_PersonGroup_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_PersonGroup_Group] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[Group]([Id])
)
GO

-- Message/Communication logs
CREATE TABLE [dbo].[MessageLog]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [SenderId] UNIQUEIDENTIFIER NOT NULL,
    [ReceiverId] UNIQUEIDENTIFIER NULL,                -- NULL for broadcast messages
    [GroupId] UNIQUEIDENTIFIER NULL,                   -- NULL for direct messages
    [MessageType] NVARCHAR(50) NOT NULL,               -- Direct, Group, System, etc.
    [Content] NVARCHAR(MAX) NOT NULL,
    [ContentType] NVARCHAR(50) NOT NULL,               -- Text, HTML, JSON, etc.
    [SentAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [DeliveredAt] DATETIME2 NULL,
    [ReadAt] DATETIME2 NULL,
    [Status] NVARCHAR(50) NOT NULL,                    -- Sent, Delivered, Read, Failed, etc.
    CONSTRAINT [FK_MessageLog_Sender] FOREIGN KEY ([SenderId]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_MessageLog_Receiver] FOREIGN KEY ([ReceiverId]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_MessageLog_Group] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[Group]([Id])
)
GO

-- Indexes for performance
CREATE INDEX [IX_Person_Email] ON [dbo].[Person] ([Email])
GO

CREATE INDEX [IX_Person_Names] ON [dbo].[Person] ([LastName], [FirstName])
GO

CREATE INDEX [IX_Person_ExternalId] ON [dbo].[Person] ([ExternalId]) WHERE [ExternalId] IS NOT NULL
GO

CREATE INDEX [IX_PersonContact_PersonId] ON [dbo].[PersonContact] ([PersonId])
GO

CREATE INDEX [IX_PersonContact_Type_Value] ON [dbo].[PersonContact] ([ContactType], [Value])
GO

CREATE INDEX [IX_PersonRelationship_PersonId] ON [dbo].[PersonRelationship] ([PersonId])
GO

CREATE INDEX [IX_PersonRelationship_RelatedPersonId] ON [dbo].[PersonRelationship] ([RelatedPersonId])
GO

CREATE INDEX [IX_PersonGroup_PersonId] ON [dbo].[PersonGroup] ([PersonId])
GO

CREATE INDEX [IX_PersonGroup_GroupId] ON [dbo].[PersonGroup] ([GroupId])
GO

CREATE INDEX [IX_MessageLog_SenderId] ON [dbo].[MessageLog] ([SenderId])
GO

CREATE INDEX [IX_MessageLog_ReceiverId] ON [dbo].[MessageLog] ([ReceiverId]) WHERE [ReceiverId] IS NOT NULL
GO

CREATE INDEX [IX_MessageLog_GroupId] ON [dbo].[MessageLog] ([GroupId]) WHERE [GroupId] IS NOT NULL
GO

CREATE INDEX [IX_MessageLog_SentAt] ON [dbo].[MessageLog] ([SentAt])
GO
