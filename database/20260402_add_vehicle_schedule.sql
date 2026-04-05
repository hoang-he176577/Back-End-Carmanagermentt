-- Vehicle schedule table for planned trips (DB-first)
-- Target: MSSQL (localdb)

IF OBJECT_ID('dbo.vehicle_schedule', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.vehicle_schedule
    (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        vehicle_id INT NOT NULL,
        driver_id INT NOT NULL,
        branch_id INT NOT NULL,
        planned_start_time DATETIME NOT NULL,
        planned_end_time DATETIME NOT NULL,
        actual_start_time DATETIME NULL,
        actual_end_time DATETIME NULL,
        origin NVARCHAR(255) NULL,
        destination NVARCHAR(255) NULL,
        status NVARCHAR(30) NOT NULL CONSTRAINT DF_vehicle_schedule_status DEFAULT ('Planned'),
        extension_minutes INT NULL,
        extension_reason NVARCHAR(500) NULL,
        swap_from_schedule_id INT NULL,
        swapped_vehicle_id INT NULL,
        created_at DATETIME NULL CONSTRAINT DF_vehicle_schedule_created_at DEFAULT (GETDATE()),
        updated_at DATETIME NULL,
        deleted_at DATETIME NULL
    );

    ALTER TABLE dbo.vehicle_schedule
        ADD CONSTRAINT CK_vehicle_schedule_time
        CHECK (planned_end_time > planned_start_time);

    ALTER TABLE dbo.vehicle_schedule
        ADD CONSTRAINT FK_vehicle_schedule_vehicle
        FOREIGN KEY (vehicle_id) REFERENCES dbo.vehicle(id);

    ALTER TABLE dbo.vehicle_schedule
        ADD CONSTRAINT FK_vehicle_schedule_driver
        FOREIGN KEY (driver_id) REFERENCES dbo.driver(id);

    ALTER TABLE dbo.vehicle_schedule
        ADD CONSTRAINT FK_vehicle_schedule_branch
        FOREIGN KEY (branch_id) REFERENCES dbo.branch(id);

    ALTER TABLE dbo.vehicle_schedule
        ADD CONSTRAINT FK_vehicle_schedule_swapped_vehicle
        FOREIGN KEY (swapped_vehicle_id) REFERENCES dbo.vehicle(id);

    ALTER TABLE dbo.vehicle_schedule
        ADD CONSTRAINT FK_vehicle_schedule_swap_from
        FOREIGN KEY (swap_from_schedule_id) REFERENCES dbo.vehicle_schedule(id);

    CREATE INDEX IX_vehicle_schedule_vehicle_time
        ON dbo.vehicle_schedule(vehicle_id, planned_start_time, planned_end_time);

    CREATE INDEX IX_vehicle_schedule_driver_time
        ON dbo.vehicle_schedule(driver_id, planned_start_time, planned_end_time);

    CREATE INDEX IX_vehicle_schedule_branch_time
        ON dbo.vehicle_schedule(branch_id, planned_start_time, planned_end_time);
END
