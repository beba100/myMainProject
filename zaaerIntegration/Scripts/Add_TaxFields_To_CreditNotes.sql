-- Adds tax-related columns to credit_notes table if they don't exist
-- VAT Rate, VAT Amount, Lodging Tax Rate, Lodging Tax Amount

-- Add vat_rate column
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.objects t ON t.object_id = c.object_id
    WHERE t.name = 'credit_notes' AND c.name = 'vat_rate'
)
BEGIN
    ALTER TABLE dbo.credit_notes
    ADD vat_rate DECIMAL(12,4) NULL;
    PRINT 'Added vat_rate column to credit_notes table';
END
ELSE
BEGIN
    PRINT 'vat_rate column already exists in credit_notes table';
END
GO

-- Add vat_amount column
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.objects t ON t.object_id = c.object_id
    WHERE t.name = 'credit_notes' AND c.name = 'vat_amount'
)
BEGIN
    ALTER TABLE dbo.credit_notes
    ADD vat_amount DECIMAL(12,2) NULL;
    PRINT 'Added vat_amount column to credit_notes table';
END
ELSE
BEGIN
    PRINT 'vat_amount column already exists in credit_notes table';
END
GO

-- Add lodging_tax_rate column
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.objects t ON t.object_id = c.object_id
    WHERE t.name = 'credit_notes' AND c.name = 'lodging_tax_rate'
)
BEGIN
    ALTER TABLE dbo.credit_notes
    ADD lodging_tax_rate DECIMAL(12,4) NULL;
    PRINT 'Added lodging_tax_rate column to credit_notes table';
END
ELSE
BEGIN
    PRINT 'lodging_tax_rate column already exists in credit_notes table';
END
GO

-- Add lodging_tax_amount column
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.objects t ON t.object_id = c.object_id
    WHERE t.name = 'credit_notes' AND c.name = 'lodging_tax_amount'
)
BEGIN
    ALTER TABLE dbo.credit_notes
    ADD lodging_tax_amount DECIMAL(12,2) NULL;
    PRINT 'Added lodging_tax_amount column to credit_notes table';
END
ELSE
BEGIN
    PRINT 'lodging_tax_amount column already exists in credit_notes table';
END
GO

-- Add subtotal column (if not already added)
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.objects t ON t.object_id = c.object_id
    WHERE t.name = 'credit_notes' AND c.name = 'subtotal'
)
BEGIN
    ALTER TABLE dbo.credit_notes
    ADD subtotal DECIMAL(12,2) NULL;
    PRINT 'Added subtotal column to credit_notes table';
END
ELSE
BEGIN
    PRINT 'subtotal column already exists in credit_notes table';
END
GO

PRINT 'Migration completed successfully!';
GO

