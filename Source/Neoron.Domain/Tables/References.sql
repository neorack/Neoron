CREATE TABLE [dbo].[RefGender]
(
    [Id] TINYINT NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(200) NULL
)
GO

CREATE TABLE [dbo].[RefContactType]
(
    [Id] TINYINT NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(200) NULL
)
GO
