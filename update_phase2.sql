USE [CarManager];
GO

-- 1. Them Version vao BulkPurchaseDetail
IF COL_LENGTH('dbo.bulk_purchase_detail', 'version') IS NULL
BEGIN
    ALTER TABLE [dbo].[bulk_purchase_detail] ADD [version] NVARCHAR(100) NULL;
END
GO

-- 2. Them Version vao VehicleReceptionRecord
IF COL_LENGTH('dbo.vehicle_reception_record', 'version') IS NULL
BEGIN
    ALTER TABLE [dbo].[vehicle_reception_record] ADD [version] NVARCHAR(100) NULL;
END
GO

-- 3. Them ActualCost vao PurchaseProposal
IF COL_LENGTH('dbo.purchase_proposal', 'actual_cost') IS NULL
BEGIN
    ALTER TABLE [dbo].[purchase_proposal] ADD [actual_cost] DECIMAL(15,2) NULL;
END
GO

-- 4. Them fuel_norm vao bulk_purchase_detail
IF COL_LENGTH('dbo.bulk_purchase_detail', 'fuel_norm') IS NULL
BEGIN
    ALTER TABLE bulk_purchase_detail ADD fuel_norm DECIMAL(5,2);
END
GO

-- [PHASE 2] Bổ sung cột hạn hoàn thành đề xuất
IF COL_LENGTH('dbo.purchase_proposal', 'completion_deadline') IS NULL
BEGIN
    ALTER TABLE purchase_proposal ADD completion_deadline DATETIME NULL;
END
GO

-- [Transfer Plan] Bổ sung các cột xác nhận điều chuyển (Check-out/Check-in)
IF COL_LENGTH('dbo.transfer_plan', 'checkout_date') IS NULL
BEGIN
    ALTER TABLE [dbo].[transfer_plan] ADD [checkout_date] DATETIME NULL;
END
GO

IF COL_LENGTH('dbo.transfer_plan', 'checkout_by_user_id') IS NULL
BEGIN
    ALTER TABLE [dbo].[transfer_plan] ADD [checkout_by_user_id] INT NULL;
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_transfer_plan_checkout_by')
    BEGIN
        ALTER TABLE [dbo].[transfer_plan] ADD CONSTRAINT [FK_transfer_plan_checkout_by] FOREIGN KEY ([checkout_by_user_id]) REFERENCES [dbo].[user] ([id]);
    END
END
GO

IF COL_LENGTH('dbo.transfer_plan', 'checkin_date') IS NULL
BEGIN
    ALTER TABLE [dbo].[transfer_plan] ADD [checkin_date] DATETIME NULL;
END
GO

IF COL_LENGTH('dbo.transfer_plan', 'checkin_by_user_id') IS NULL
BEGIN
    ALTER TABLE [dbo].[transfer_plan] ADD [checkin_by_user_id] INT NULL;
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_transfer_plan_checkin_by')
    BEGIN
        ALTER TABLE [dbo].[transfer_plan] ADD CONSTRAINT [FK_transfer_plan_checkin_by] FOREIGN KEY ([checkin_by_user_id]) REFERENCES [dbo].[user] ([id]);
    END
END
GO

-- [PHASE 2 - UPDATE] Bổ sung Năm sản xuất và Số KM vào Đối chiếu và Kho tài sản
IF COL_LENGTH('dbo.vehicle_reception_record', 'year_manufacture') IS NULL
BEGIN
    ALTER TABLE [dbo].[vehicle_reception_record] ADD [year_manufacture] INT NULL;
END
GO

IF COL_LENGTH('dbo.vehicle_reception_record', 'mileage') IS NULL
BEGIN
    ALTER TABLE [dbo].[vehicle_reception_record] ADD [mileage] DECIMAL(18,2) NULL;
END
GO

IF COL_LENGTH('dbo.vehicle', 'year_manufacture') IS NULL
BEGIN
    ALTER TABLE [dbo].[vehicle] ADD [year_manufacture] INT NULL;
END
GO

IF COL_LENGTH('dbo.vehicle', 'mileage') IS NULL
BEGIN
    ALTER TABLE [dbo].[vehicle] ADD [mileage] DECIMAL(18,2) NULL;
END
GO

-- [PHASE 2 - UPDATE 2] Bổ sung cấu hình kỹ thuật vào VehicleModel
IF COL_LENGTH('dbo.vehicle_model', 'engine_power') IS NULL
BEGIN
    ALTER TABLE [dbo].[vehicle_model] ADD [engine_power] NVARCHAR(100) NULL;
END
GO

IF COL_LENGTH('dbo.vehicle_model', 'emission_standard') IS NULL
BEGIN
    ALTER TABLE [dbo].[vehicle_model] ADD [emission_standard] NVARCHAR(50) NULL;
END
GO

IF COL_LENGTH('dbo.vehicle_model', 'payload_capacity') IS NULL
BEGIN
    ALTER TABLE [dbo].[vehicle_model] ADD [payload_capacity] DECIMAL(18,2) NULL;
END
GO

IF COL_LENGTH('dbo.vehicle_model', 'fuel_type') IS NULL
BEGIN
    ALTER TABLE [dbo].[vehicle_model] ADD [fuel_type] NVARCHAR(50) NULL;
END
GO
