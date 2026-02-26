
USE [CarManager];
GO

IF COL_LENGTH('dbo.vehicle', 'Vin') IS NULL
BEGIN
    ALTER TABLE dbo.vehicle
        ADD Vin NVARCHAR(50) NULL;
END;
GO

IF COL_LENGTH('dbo.vehicle', 'EngineNumber') IS NULL
BEGIN
    ALTER TABLE dbo.vehicle
        ADD EngineNumber NVARCHAR(50) NULL;
END;
GO

IF COL_LENGTH('dbo.vehicle', 'ChassisNumber') IS NULL
BEGIN
    ALTER TABLE dbo.vehicle
        ADD ChassisNumber NVARCHAR(50) NULL;
END;
GO

IF COL_LENGTH('dbo.vehicle', 'Color') IS NULL
BEGIN
    ALTER TABLE dbo.vehicle
        ADD Color NVARCHAR(50) NULL;
END;
GO

IF COL_LENGTH('dbo.vehicle', 'SeatCount') IS NULL
BEGIN
    ALTER TABLE dbo.vehicle
        ADD SeatCount INT NULL;
END;
GO

IF COL_LENGTH('dbo.vehicle', 'FuelType') IS NULL
BEGIN
    ALTER TABLE dbo.vehicle
        ADD FuelType NVARCHAR(30) NULL;
END;
GO

IF COL_LENGTH('dbo.vehicle', 'WarrantyExpiryDate') IS NULL
BEGIN
    ALTER TABLE dbo.vehicle
        ADD WarrantyExpiryDate DATE NULL;
END;
GO

-- ==============================
-- 2. Unique index cho VIN / EngineNumber / ChassisNumber
--    (cho phép NULL, nhưng nếu có giá trị thì không được trùng)
-- ==============================
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UQ_vehicle_Vin' AND object_id = OBJECT_ID('dbo.vehicle')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_vehicle_Vin
        ON dbo.vehicle (Vin)
        WHERE Vin IS NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UQ_vehicle_EngineNumber' AND object_id = OBJECT_ID('dbo.vehicle')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_vehicle_EngineNumber
        ON dbo.vehicle (EngineNumber)
        WHERE EngineNumber IS NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UQ_vehicle_ChassisNumber' AND object_id = OBJECT_ID('dbo.vehicle')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_vehicle_ChassisNumber
        ON dbo.vehicle (ChassisNumber)
        WHERE ChassisNumber IS NOT NULL;
END;
GO


