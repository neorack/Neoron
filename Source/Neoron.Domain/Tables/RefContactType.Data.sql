MERGE INTO [dbo].[RefContactType] AS target
USING (VALUES
    (1, 'Email', 10),
    (2, 'Phone', 20),
    (3, 'Address', 30),
    (4, 'Social', 40),
    (5, 'Other', 50)
) AS source ([Id], [Name], [DisplayOrder])
ON target.[Id] = source.[Id]
WHEN MATCHED THEN
    UPDATE SET [Name] = source.[Name], [DisplayOrder] = source.[DisplayOrder]
WHEN NOT MATCHED THEN
    INSERT ([Id], [Name], [DisplayOrder]) VALUES (source.[Id], source.[Name], source.[DisplayOrder]);
GO
