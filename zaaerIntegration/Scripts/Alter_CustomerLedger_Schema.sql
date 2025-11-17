-- =============================================
-- Alter Customer Ledger Schema
-- Adds / adjusts columns required for the ledger feature
-- Idempotent: each statement checks existence before altering
-- =============================================

PRINT '========================================';
PRINT 'Altering customer_accounts / customer_transactions schema';
PRINT '========================================';
PRINT '';

-----------------------------------------------
-- CUSTOMER_ACCOUNTS
-----------------------------------------------

IF COL_LENGTH('dbo.customer_accounts', 'reservation_id') IS NULL
BEGIN
	ALTER TABLE dbo.customer_accounts ADD reservation_id INT NULL;
	PRINT '✓ Added customer_accounts.reservation_id';
END

IF COL_LENGTH('dbo.customer_accounts', 'currency_code') IS NULL
BEGIN
	ALTER TABLE dbo.customer_accounts ADD currency_code NVARCHAR(10) NULL;
	UPDATE dbo.customer_accounts SET currency_code = 'SAR' WHERE currency_code IS NULL;
	PRINT '✓ Added customer_accounts.currency_code';
END

IF COL_LENGTH('dbo.customer_accounts', 'total_credit') IS NULL
BEGIN
	ALTER TABLE dbo.customer_accounts ADD total_credit DECIMAL(18,2) NOT NULL CONSTRAINT DF_customer_accounts_total_credit DEFAULT (0);
	PRINT '✓ Added customer_accounts.total_credit';
END

IF COL_LENGTH('dbo.customer_accounts', 'total_debit') IS NULL
BEGIN
	ALTER TABLE dbo.customer_accounts ADD total_debit DECIMAL(18,2) NOT NULL CONSTRAINT DF_customer_accounts_total_debit DEFAULT (0);
	PRINT '✓ Added customer_accounts.total_debit';
END

IF COL_LENGTH('dbo.customer_accounts', 'last_transaction_at') IS NULL
BEGIN
	ALTER TABLE dbo.customer_accounts ADD last_transaction_at DATETIME2(7) NULL;
	PRINT '✓ Added customer_accounts.last_transaction_at';
END

IF COL_LENGTH('dbo.customer_accounts', 'status') IS NULL
BEGIN
	ALTER TABLE dbo.customer_accounts ADD status NVARCHAR(20) NOT NULL CONSTRAINT DF_customer_accounts_status DEFAULT ('active');
	PRINT '✓ Added customer_accounts.status';
END

IF COL_LENGTH('dbo.customer_accounts', 'created_at') IS NULL
BEGIN
	ALTER TABLE dbo.customer_accounts ADD created_at DATETIME2(7) NOT NULL CONSTRAINT DF_customer_accounts_created_at DEFAULT (SYSUTCDATETIME());
	PRINT '✓ Added customer_accounts.created_at';
END

IF COL_LENGTH('dbo.customer_accounts', 'zaaer_id') IS NULL
BEGIN
	ALTER TABLE dbo.customer_accounts ADD zaaer_id INT NULL;
	PRINT '✓ Added customer_accounts.zaaer_id';
END

-- Ensure balance column has required precision
ALTER TABLE dbo.customer_accounts ALTER COLUMN balance DECIMAL(18,2) NOT NULL;
PRINT '✓ Altered customer_accounts.balance to DECIMAL(18,2)';

-----------------------------------------------
-- CUSTOMER_TRANSACTIONS
-----------------------------------------------

IF COL_LENGTH('dbo.customer_transactions', 'account_id') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD account_id INT NULL;
	PRINT '✓ Added customer_transactions.account_id';
END

IF COL_LENGTH('dbo.customer_transactions', 'customer_id') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD customer_id INT NOT NULL CONSTRAINT DF_customer_transactions_customer_id DEFAULT (0);
	PRINT '✓ Added customer_transactions.customer_id';
END

IF COL_LENGTH('dbo.customer_transactions', 'reservation_id') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD reservation_id INT NULL;
	PRINT '✓ Added customer_transactions.reservation_id';
END

IF COL_LENGTH('dbo.customer_transactions', 'hotel_id') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD hotel_id INT NOT NULL CONSTRAINT DF_customer_transactions_hotel_id DEFAULT (0);
	PRINT '✓ Added customer_transactions.hotel_id';
END

IF COL_LENGTH('dbo.customer_transactions', 'payment_receipt_id') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD payment_receipt_id INT NULL;
	PRINT '✓ Added customer_transactions.payment_receipt_id';
END

IF COL_LENGTH('dbo.customer_transactions', 'CreditNoteId') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD CreditNoteId INT NULL;
	PRINT '✓ Added customer_transactions.CreditNoteId (compatibility column)';
END

IF COL_LENGTH('dbo.customer_transactions', 'receipt_no') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD receipt_no NVARCHAR(50) NULL;
	PRINT '✓ Added customer_transactions.receipt_no';
END

IF COL_LENGTH('dbo.customer_transactions', 'voucher_code') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD voucher_code NVARCHAR(50) NULL;
	PRINT '✓ Added customer_transactions.voucher_code';
END

IF COL_LENGTH('dbo.customer_transactions', 'receipt_type') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD receipt_type NVARCHAR(50) NULL;
	PRINT '✓ Added customer_transactions.receipt_type';
END

IF COL_LENGTH('dbo.customer_transactions', 'zaaer_receipt_id') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD zaaer_receipt_id INT NULL;
	PRINT '✓ Added customer_transactions.zaaer_receipt_id';
END

IF COL_LENGTH('dbo.customer_transactions', 'transaction_date') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD transaction_date DATETIME2(7) NOT NULL CONSTRAINT DF_customer_transactions_transaction_date DEFAULT (SYSUTCDATETIME());
	PRINT '✓ Added customer_transactions.transaction_date';
END

IF COL_LENGTH('dbo.customer_transactions', 'transaction_type') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD transaction_type NVARCHAR(50) NOT NULL CONSTRAINT DF_customer_transactions_transaction_type DEFAULT ('receipt');
	PRINT '✓ Added customer_transactions.transaction_type';
END

IF COL_LENGTH('dbo.customer_transactions', 'transaction_source') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD transaction_source NVARCHAR(30) NOT NULL CONSTRAINT DF_customer_transactions_transaction_source DEFAULT ('PaymentReceipt');
	PRINT '✓ Added customer_transactions.transaction_source';
END

IF COL_LENGTH('dbo.customer_transactions', 'transaction_status') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD transaction_status NVARCHAR(20) NOT NULL CONSTRAINT DF_customer_transactions_transaction_status DEFAULT ('active');
	PRINT '✓ Added customer_transactions.transaction_status';
END

IF COL_LENGTH('dbo.customer_transactions', 'credit_amount') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD credit_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_customer_transactions_credit_amount DEFAULT (0);
	PRINT '✓ Added customer_transactions.credit_amount';
END

IF COL_LENGTH('dbo.customer_transactions', 'debit_amount') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD debit_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_customer_transactions_debit_amount DEFAULT (0);
	PRINT '✓ Added customer_transactions.debit_amount';
END

IF COL_LENGTH('dbo.customer_transactions', 'balance_after') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD balance_after DECIMAL(18,2) NOT NULL CONSTRAINT DF_customer_transactions_balance_after DEFAULT (0);
	PRINT '✓ Added customer_transactions.balance_after';
END

IF COL_LENGTH('dbo.customer_transactions', 'payment_method') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD payment_method NVARCHAR(50) NULL;
	PRINT '✓ Added customer_transactions.payment_method';
END

IF COL_LENGTH('dbo.customer_transactions', 'description') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD description NVARCHAR(255) NULL;
	PRINT '✓ Added customer_transactions.description';
END

IF COL_LENGTH('dbo.customer_transactions', 'created_at') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD created_at DATETIME2(7) NOT NULL CONSTRAINT DF_customer_transactions_created_at DEFAULT (SYSUTCDATETIME());
	PRINT '✓ Added customer_transactions.created_at';
END

IF COL_LENGTH('dbo.customer_transactions', 'updated_by') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD updated_by INT NULL;
	PRINT '✓ Added customer_transactions.updated_by';
END

IF COL_LENGTH('dbo.customer_transactions', 'updated_at') IS NULL
BEGIN
	ALTER TABLE dbo.customer_transactions ADD updated_at DATETIME2(7) NULL;
	PRINT '✓ Added customer_transactions.updated_at';
END

-- Default existing data
IF COL_LENGTH('dbo.customer_transactions', 'amount') IS NOT NULL
BEGIN
	UPDATE dbo.customer_transactions
	SET
		transaction_status = ISNULL(transaction_status, 'active'),
		transaction_source = ISNULL(transaction_source, 'PaymentReceipt'),
		credit_amount = CASE WHEN credit_amount IS NULL THEN CASE WHEN amount < 0 THEN 0 ELSE ISNULL(amount, 0) END ELSE credit_amount END,
		debit_amount = CASE WHEN debit_amount IS NULL THEN CASE WHEN amount < 0 THEN ABS(amount) ELSE 0 END ELSE debit_amount END;
END
ELSE
BEGIN
	UPDATE dbo.customer_transactions
	SET
		transaction_status = ISNULL(transaction_status, 'active'),
		transaction_source = ISNULL(transaction_source, 'PaymentReceipt'),
		credit_amount = ISNULL(credit_amount, 0),
		debit_amount = ISNULL(debit_amount, 0);
END

-----------------------------------------------
-- INDEXES
-----------------------------------------------

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_customer_transactions_account_date' AND object_id = OBJECT_ID('dbo.customer_transactions'))
BEGIN
	CREATE NONCLUSTERED INDEX IX_customer_transactions_account_date
	ON dbo.customer_transactions(account_id, transaction_date);
	PRINT '✓ Created index IX_customer_transactions_account_date';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_customer_transactions_receipt' AND object_id = OBJECT_ID('dbo.customer_transactions'))
BEGIN
	CREATE NONCLUSTERED INDEX IX_customer_transactions_receipt
	ON dbo.customer_transactions(payment_receipt_id);
	PRINT '✓ Created index IX_customer_transactions_receipt';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_customer_accounts_customer_reservation' AND object_id = OBJECT_ID('dbo.customer_accounts'))
BEGIN
	CREATE NONCLUSTERED INDEX IX_customer_accounts_customer_reservation
	ON dbo.customer_accounts(customer_id, hotel_id, reservation_id);
	PRINT '✓ Created index IX_customer_accounts_customer_reservation';
END

PRINT '';
PRINT 'Schema alteration completed. Review totals with CustomerLedgerService.RecalculateAccountAsync when application runs.';
PRINT '========================================';

