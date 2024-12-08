CREATE TABLE [dbo].[RefGender]
(
    [Id] TINYINT NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL UNIQUE,
    [DisplayOrder] TINYINT NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1
)
GO

INSERT INTO [dbo].[RefGender] ([Id], [Name], [DisplayOrder]) VALUES
(1, 'Male', 10),
(2, 'Female', 20),
(3, 'Non-Binary', 30),
(4, 'Other', 40),
(5, 'Prefer Not To Say', 50)
GO
