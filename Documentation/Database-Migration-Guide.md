# Database Migration Guide

## Overview
This guide covers database migration procedures for the Neoron API, including both development and production environments.

## Migration Scripts

### Initial Schema
```sql
-- Initial database schema
CREATE TABLE Messages (
    MessageId BIGINT PRIMARY KEY,
    ChannelId BIGINT NOT NULL,
    GuildId BIGINT NOT NULL,
    AuthorId BIGINT NOT NULL,
    Content NVARCHAR(MAX),
    MessageType INT NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL,
    EditedAt DATETIMEOFFSET NULL,
    DeletedAt DATETIMEOFFSET NULL,
    INDEX IX_Messages_ChannelId (ChannelId),
    INDEX IX_Messages_GuildId (GuildId),
    INDEX IX_Messages_AuthorId (AuthorId)
);

-- Audit table
CREATE TABLE MessageAudits (
    AuditId BIGINT IDENTITY(1,1) PRIMARY KEY,
    MessageId BIGINT NOT NULL,
    Action VARCHAR(50) NOT NULL,
    ChangedBy BIGINT NOT NULL,
    ChangedAt DATETIMEOFFSET NOT NULL,
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    FOREIGN KEY (MessageId) REFERENCES Messages(MessageId)
);
```

### Version Updates
```sql
-- V1.1: Add message references
ALTER TABLE Messages
ADD ReplyToId BIGINT NULL,
    ThreadId BIGINT NULL;

ALTER TABLE Messages
ADD CONSTRAINT FK_Messages_ReplyTo 
    FOREIGN KEY (ReplyToId) REFERENCES Messages(MessageId);

ALTER TABLE Messages
ADD CONSTRAINT FK_Messages_Thread 
    FOREIGN KEY (ThreadId) REFERENCES Messages(MessageId);

-- V1.2: Add message metadata
ALTER TABLE Messages
ADD EmbeddedContent NVARCHAR(MAX) NULL,
    Attachments NVARCHAR(MAX) NULL,
    Flags INT NOT NULL DEFAULT 0;
```

## Migration Procedures

### Development Environment
1. Create migration:
   ```bash
   dotnet ef migrations add <MigrationName> --project Source/Neoron.API
   ```

2. Apply migration:
   ```bash
   dotnet ef database update --project Source/Neoron.API
   ```

3. Generate script:
   ```bash
   dotnet ef migrations script --project Source/Neoron.API --output Scripts/Migration.sql
   ```

### Production Environment
1. Generate idempotent script:
   ```bash
   dotnet ef migrations script --idempotent --project Source/Neoron.API --output Scripts/Production.sql
   ```

2. Review script for:
   - Data loss risks
   - Performance impact
   - Locking considerations

3. Execute with proper backup:
   ```sql
   BACKUP DATABASE [Neoron] TO DISK = 'C:\Backups\Pre_Migration.bak'
   GO
   
   BEGIN TRANSACTION
   
   -- Execute migration script here
   
   -- Verify data integrity
   -- Run validation queries
   
   COMMIT TRANSACTION
   ```

## Rollback Procedures

### Quick Rollback
```sql
-- Revert last migration
dotnet ef migrations remove --project Source/Neoron.API

-- Generate rollback script
dotnet ef migrations script <PreviousVersion> <CurrentVersion> --project Source/Neoron.API
```

### Emergency Rollback
```sql
-- Restore from backup
RESTORE DATABASE [Neoron] FROM DISK = 'C:\Backups\Pre_Migration.bak'
WITH REPLACE, RECOVERY;
```

## Data Migration

### Export Data
```sql
-- Export to JSON
SELECT *
FROM Messages
FOR JSON PATH, ROOT('messages')
```

### Import Data
```sql
-- Import from staging
INSERT INTO Messages (MessageId, ChannelId, GuildId, AuthorId, Content, MessageType, CreatedAt)
SELECT MessageId, ChannelId, GuildId, AuthorId, Content, MessageType, CreatedAt
FROM StagingMessages;
```

## Verification Steps

### Pre-Migration
1. Check database size and growth
   - Current database size
   - Growth rate analysis
   - Storage capacity planning
   - Index fragmentation levels
2. Verify backup status
   - Latest successful backup
   - Backup integrity verification
   - Recovery time objectives (RTO)
   - Recovery point objectives (RPO)
3. Test migration in staging
   - Full rehearsal in staging environment
   - Performance impact analysis
   - Data integrity verification
   - Application compatibility testing
   - Rollback procedure verification

### Post-Migration
1. Verify data integrity
2. Check index performance
3. Validate foreign keys
4. Test application functionality

## Monitoring

### Key Metrics
- Migration duration
- Table sizes
- Index fragmentation
- Query performance

### Alerts
- Long-running queries
- Blocking operations
- Error conditions
