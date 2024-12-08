MERGE INTO [dbo].[RefGender] AS target
USING (VALUES
    (1, 'Male', 10),
    (2, 'Female', 20),
    (3, 'Non-Binary', 30),
    (4, 'Other', 40),
    (5, 'Prefer Not To Say', 50)
) AS source ([Id], [Name], [DisplayOrder])
ON target.[Id] = source.[Id]
WHEN MATCHED THEN
    UPDATE SET [Name] = source.[Name], [DisplayOrder] = source.[DisplayOrder]
WHEN NOT MATCHED THEN
    INSERT ([Id], [Name], [DisplayOrder]) VALUES (source.[Id], source.[Name], source.[DisplayOrder]);
GO
