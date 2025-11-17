-- Adds receipt_status column to payment_receipts if it doesn't exist
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.objects t ON t.object_id = c.object_id
    WHERE t.name = 'payment_receipts' AND c.name = 'receipt_status'
)
BEGIN
    ALTER TABLE dbo.payment_receipts
    ADD receipt_status NVARCHAR(50) NULL CONSTRAINT DF_payment_receipts_receipt_status DEFAULT ('active');
END
GO

-- Backfill existing rows to 'active' if null
UPDATE dbo.payment_receipts
SET receipt_status = 'active'
WHERE receipt_status IS NULL;
GO

