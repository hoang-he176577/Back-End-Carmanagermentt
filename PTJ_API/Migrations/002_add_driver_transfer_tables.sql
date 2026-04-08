-- ============================================================
-- Migration: Add driver transfer request/detail tables
-- Run this on the CarManager database
-- ============================================================

IF OBJECT_ID('dbo.driver_transfer_request', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[driver_transfer_request]
    (
        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [requesting_branch_id] INT NOT NULL,
        [requested_quantity] INT NOT NULL,
        [fulfilled_quantity] INT NOT NULL CONSTRAINT [DF_driver_transfer_request_fulfilled_quantity] DEFAULT (0),
        [status] NVARCHAR(20) NULL CONSTRAINT [DF_driver_transfer_request_status] DEFAULT (N'Pending'),
        [reason] NVARCHAR(MAX) NULL,
        [created_by_user_id] INT NOT NULL,
        [created_at] DATETIME NULL CONSTRAINT [DF_driver_transfer_request_created_at] DEFAULT (GETDATE()),
        [updated_at] DATETIME NULL CONSTRAINT [DF_driver_transfer_request_updated_at] DEFAULT (GETDATE()),
        [deleted_at] DATETIME NULL
    );

    ALTER TABLE [dbo].[driver_transfer_request]
        ADD CONSTRAINT [FK_driver_transfer_request_branch]
            FOREIGN KEY ([requesting_branch_id]) REFERENCES [dbo].[branch]([id]);

    ALTER TABLE [dbo].[driver_transfer_request]
        ADD CONSTRAINT [FK_driver_transfer_request_user]
            FOREIGN KEY ([created_by_user_id]) REFERENCES [dbo].[user]([id]);

    CREATE INDEX [IX_driver_transfer_request_branch_status]
        ON [dbo].[driver_transfer_request]([requesting_branch_id], [status]);

    PRINT 'Table driver_transfer_request created.';
END
ELSE
BEGIN
    PRINT 'Table driver_transfer_request already exists, skipping.';
END
GO

IF OBJECT_ID('dbo.driver_transfer_detail', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[driver_transfer_detail]
    (
        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [transfer_request_id] INT NOT NULL,
        [driver_id] INT NOT NULL,
        [from_branch_id] INT NOT NULL,
        [confirmed_by_user_id] INT NOT NULL,
        [transfer_date] DATETIME NULL,
        [created_at] DATETIME NULL CONSTRAINT [DF_driver_transfer_detail_created_at] DEFAULT (GETDATE())
    );

    ALTER TABLE [dbo].[driver_transfer_detail]
        ADD CONSTRAINT [FK_driver_transfer_detail_request]
            FOREIGN KEY ([transfer_request_id]) REFERENCES [dbo].[driver_transfer_request]([id]);

    ALTER TABLE [dbo].[driver_transfer_detail]
        ADD CONSTRAINT [FK_driver_transfer_detail_driver]
            FOREIGN KEY ([driver_id]) REFERENCES [dbo].[driver]([id]);

    ALTER TABLE [dbo].[driver_transfer_detail]
        ADD CONSTRAINT [FK_driver_transfer_detail_branch]
            FOREIGN KEY ([from_branch_id]) REFERENCES [dbo].[branch]([id]);

    ALTER TABLE [dbo].[driver_transfer_detail]
        ADD CONSTRAINT [FK_driver_transfer_detail_user]
            FOREIGN KEY ([confirmed_by_user_id]) REFERENCES [dbo].[user]([id]);

    CREATE INDEX [IX_driver_transfer_detail_request]
        ON [dbo].[driver_transfer_detail]([transfer_request_id]);

    CREATE INDEX [IX_driver_transfer_detail_driver]
        ON [dbo].[driver_transfer_detail]([driver_id]);

    PRINT 'Table driver_transfer_detail created.';
END
ELSE
BEGIN
    PRINT 'Table driver_transfer_detail already exists, skipping.';
END
GO
