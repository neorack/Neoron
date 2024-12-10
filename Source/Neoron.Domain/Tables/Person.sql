CREATE TABLE [dbo].[Person]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [ExternalId] NVARCHAR(100) NULL,                    -- For external system integration
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [MiddleName] NVARCHAR(100) NULL,
    [PreferredName] NVARCHAR(100) NULL,
    [DateOfBirth] DATE NULL,
    [GenderId] TINYINT NULL,
    CONSTRAINT [FK_Person_Gender] FOREIGN KEY ([GenderId]) REFERENCES [dbo].[RefGender]([Id]),
    [TimeZone] NVARCHAR(100) NULL,                      -- For global user base
    [PreferredLanguage] NCHAR(5) NULL,                  -- ISO language code
    [LastLoginAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] UNIQUEIDENTIFIER NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [DeletedAt] DATETIME2 NULL,                         -- Soft delete support
    [DeletedBy] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [FK_Person_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Person]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Person_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[Person]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Person_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[Person]([Id]) ON DELETE NO ACTION
)
GO

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

-- Groups table
CREATE TABLE [dbo].[UserGroup] -- Renamed from Group to avoid reserved word
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

-- Person-Group memberships
CREATE TABLE [dbo].[PersonGroup]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [GroupId] UNIQUEIDENTIFIER NOT NULL,
    [Role] NVARCHAR(50) NOT NULL CHECK ([Role] IN ('Member', 'Admin', 'Moderator', 'Owner')),
    [JoinedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ExpiresAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_PersonGroup_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_PersonGroup_Group] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[UserGroup]([Id]) ON DELETE CASCADE
)
GO


-- Person Contact Information
CREATE TABLE [dbo].[PersonContact]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [ContactTypeId] TINYINT NOT NULL,
    [Value] NVARCHAR(256) NOT NULL,                     -- Adjusted length for contact info
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [IsVerified] BIT NOT NULL DEFAULT 0,
    [VerifiedAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_PersonContact_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PersonContact_ContactType] FOREIGN KEY ([ContactTypeId]) REFERENCES [dbo].[RefContactType]([Id])
)
GO

CREATE INDEX [IX_PersonContact_PersonId] ON [dbo].[PersonContact] ([PersonId])
GO

CREATE INDEX [IX_PersonContact_Type_Value] ON [dbo].[PersonContact] ([ContactTypeId], [Value]) 
    INCLUDE ([PersonId]) WHERE LEN([Value]) <= 900     -- Include PersonId and limit indexed value length
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

-- Ideology/Belief System table
CREATE TABLE [dbo].[Ideology]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Category] NVARCHAR(50) NOT NULL CHECK ([Category] IN ('Political', 'Religious', 'Philosophical', 'Social', 'Economic', 'Other')),
    [ParentId] UNIQUEIDENTIFIER NULL,                   -- For hierarchy of beliefs
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Ideology_Parent] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Ideology]([Id])
)
GO

-- Person-Ideology associations
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

-- Tags for flexible categorization
CREATE TABLE [dbo].[Tag]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [Name] NVARCHAR(100) NOT NULL UNIQUE,
    [Category] NVARCHAR(50) NOT NULL,                   -- Skills, Interests, Traits, etc.
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [FK_Tag_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_Tag_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[Person]([Id])
)
GO

-- Person-Tag associations
CREATE TABLE [dbo].[PersonTag]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [TagId] UNIQUEIDENTIFIER NOT NULL,
    [Weight] TINYINT NULL CHECK ([Weight] BETWEEN 1 AND 100),
    [Source] NVARCHAR(50) NOT NULL CHECK ([Source] IN ('Self', 'System', 'Peer', 'AI')),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [FK_PersonTag_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id]),
    CONSTRAINT [FK_PersonTag_Tag] FOREIGN KEY ([TagId]) REFERENCES [dbo].[Tag]([Id]),
    CONSTRAINT [FK_PersonTag_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Person]([Id])
)
GO

-- Activity/Event logging
CREATE TABLE [dbo].[ActivityLog]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [PersonId] UNIQUEIDENTIFIER NOT NULL,
    [ActivityType] NVARCHAR(50) NOT NULL CHECK ([ActivityType] IN ('Login', 'Message', 'GroupJoin', 'IdeologyChange', 'RelationshipChange', 'ProfileUpdate', 'Other')),
    [Description] NVARCHAR(MAX) NULL,
    [Metadata] NVARCHAR(MAX) NULL,                      -- JSON data for flexible storage
    [IpAddress] NVARCHAR(45) NULL,                      -- IPv6 compatible
    [UserAgent] NVARCHAR(500) NULL,
    [OccurredAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_ActivityLog_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id])
)
GO

-- Influence/Reach metrics
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

-- Add necessary indexes
CREATE INDEX [IX_Ideology_Category] ON [dbo].[Ideology] ([Category])
GO

CREATE INDEX [IX_PersonIdeology_PersonId] ON [dbo].[PersonIdeology] ([PersonId])
GO

CREATE INDEX [IX_PersonIdeology_IdeologyId] ON [dbo].[PersonIdeology] ([IdeologyId])
GO

CREATE INDEX [IX_Tag_Category] ON [dbo].[Tag] ([Category])
GO

CREATE INDEX [IX_PersonTag_PersonId] ON [dbo].[PersonTag] ([PersonId])
GO

CREATE INDEX [IX_PersonTag_TagId] ON [dbo].[PersonTag] ([TagId])
GO

CREATE INDEX [IX_ActivityLog_PersonId] ON [dbo].[ActivityLog] ([PersonId])
GO

CREATE INDEX [IX_ActivityLog_ActivityType] ON [dbo].[ActivityLog] ([ActivityType])
GO

CREATE INDEX [IX_ActivityLog_OccurredAt] ON [dbo].[ActivityLog] ([OccurredAt])
GO

CREATE INDEX [IX_PersonInfluence_PersonId] ON [dbo].[PersonInfluence] ([PersonId])
GO
