CREATE TABLE [dbo].[RefContactType]
(
    [Id] TINYINT NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL UNIQUE,
    [DisplayOrder] TINYINT NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1
)
GO

INSERT INTO [dbo].[RefContactType] ([Id], [Name], [DisplayOrder]) VALUES
(1, 'Email', 10),
(2, 'Phone', 20),
(3, 'Address', 30),
(4, 'Social', 40),
(5, 'Other', 50)
GO
