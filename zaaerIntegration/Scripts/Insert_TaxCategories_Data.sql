-- Insert sample tax categories data
-- This script inserts VAT and EWA tax categories

-- Check if records already exist to avoid duplicates
IF NOT EXISTS (SELECT 1 FROM [dbo].[tax_categories] WHERE [name] = 'VAT')
BEGIN
    INSERT INTO [dbo].[tax_categories] (
        [name],
        [name_ar],
        [description],
        [status],
        [created_at]
    ) VALUES (
        'VAT',
        'ضريبة القيمة المضافة',
        'Value Added Tax Category',
        'active',
        GETUTCDATE()
    );
    PRINT 'Tax category VAT inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Tax category VAT already exists.';
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[tax_categories] WHERE [name] = 'EWA')
BEGIN
    INSERT INTO [dbo].[tax_categories] (
        [name],
        [name_ar],
        [description],
        [status],
        [created_at]
    ) VALUES (
        'EWA',
        'ضريبة الإقامة',
        'EWA (Lodging Tax) Category',
        'active',
        GETUTCDATE()
    );
    PRINT 'Tax category EWA inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Tax category EWA already exists.';
END
GO

-- Display inserted records
SELECT 
    [id],
    [name],
    [name_ar],
    [description],
    [status],
    [created_at]
FROM [dbo].[tax_categories]
WHERE [name] IN ('VAT', 'EWA')
ORDER BY [id];
GO

