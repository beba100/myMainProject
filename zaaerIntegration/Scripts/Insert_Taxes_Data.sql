-- Insert sample taxes data
-- This script inserts VAT (15%) and EWA (2.5%) tax records
-- Note: Update hotel_id to match your hotel settings

DECLARE @HotelId INT = 11; -- Change this to your hotel_id

-- Insert VAT tax (15%)
IF NOT EXISTS (SELECT 1 FROM [dbo].[taxes] WHERE [tax_type] = 'VAT' AND [hotel_id] = @HotelId)
BEGIN
    INSERT INTO [dbo].[taxes] (
        [zaaer_id],
        [hotel_id],
        [tax_id],
        [tax_name],
        [tax_type],
        [tax_rate],
        [method],
        [enabled],
        [tax_code],
        [apply_on],
        [status],
        [description],
        [created_at]
    ) VALUES (
        NULL, -- zaaer_id will be set by Zaaer system
        @HotelId,
        (SELECT [id] FROM [dbo].[tax_categories] WHERE [name] = 'VAT'), -- Link to VAT category
        'VAT',
        'VAT',
        15.00,
        'percentage',
        1,
        'VAT',
        NULL,
        'active',
        'Value Added Tax 15%',
        GETUTCDATE()
    );
    PRINT 'VAT tax (15%) inserted successfully for hotel_id ' + CAST(@HotelId AS VARCHAR(10));
END
ELSE
BEGIN
    PRINT 'VAT tax already exists for hotel_id ' + CAST(@HotelId AS VARCHAR(10));
END
GO

-- Insert EWA tax (2.5%)
IF NOT EXISTS (SELECT 1 FROM [dbo].[taxes] WHERE [tax_type] = 'EWA' AND [hotel_id] = @HotelId)
BEGIN
    INSERT INTO [dbo].[taxes] (
        [zaaer_id],
        [hotel_id],
        [tax_id],
        [tax_name],
        [tax_type],
        [tax_rate],
        [method],
        [enabled],
        [tax_code],
        [apply_on],
        [status],
        [description],
        [created_at]
    ) VALUES (
        NULL, -- zaaer_id will be set by Zaaer system
        @HotelId,
        (SELECT [id] FROM [dbo].[tax_categories] WHERE [name] = 'EWA'), -- Link to EWA category
        'EWA',
        'EWA',
        2.50,
        'percentage',
        1,
        'EWA',
        NULL,
        'active',
        'EWA (Lodging Tax) 2.5%',
        GETUTCDATE()
    );
    PRINT 'EWA tax (2.5%) inserted successfully for hotel_id ' + CAST(@HotelId AS VARCHAR(10));
END
ELSE
BEGIN
    PRINT 'EWA tax already exists for hotel_id ' + CAST(@HotelId AS VARCHAR(10));
END
GO

-- Display inserted records
SELECT 
    t.[id],
    t.[zaaer_id],
    t.[hotel_id],
    t.[tax_id],
    tc.[name] AS [category_name],
    t.[tax_name],
    t.[tax_type],
    t.[tax_rate],
    t.[method],
    t.[enabled],
    t.[tax_code],
    t.[apply_on],
    t.[status],
    t.[description],
    t.[created_at]
FROM [dbo].[taxes] t
LEFT JOIN [dbo].[tax_categories] tc ON t.[tax_id] = tc.[id]
WHERE t.[tax_type] IN ('VAT', 'EWA') 
  AND t.[hotel_id] = @HotelId
ORDER BY t.[tax_type];
GO

