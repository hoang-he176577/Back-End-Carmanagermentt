-- Vehicle schedule audit log (DB-first)
-- Target: MSSQL (localdb)

IF OBJECT_ID('dbo.vehicle_schedule_audit', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.vehicle_schedule_audit
    (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        schedule_id INT NOT NULL,
        actor_user_id INT NOT NULL,
        action NVARCHAR(50) NOT NULL,
        note NVARCHAR(500) NULL,
        data_json NVARCHAR(MAX) NULL,
        created_at DATETIME NULL CONSTRAINT DF_vehicle_schedule_audit_created_at DEFAULT (GETDATE())
    );

    ALTER TABLE dbo.vehicle_schedule_audit
        ADD CONSTRAINT FK_vehicle_schedule_audit_schedule
        FOREIGN KEY (schedule_id) REFERENCES dbo.vehicle_schedule(id);

    ALTER TABLE dbo.vehicle_schedule_audit
        ADD CONSTRAINT FK_vehicle_schedule_audit_user
        FOREIGN KEY (actor_user_id) REFERENCES dbo.[user](id);

    CREATE INDEX IX_vehicle_schedule_audit_schedule_time
        ON dbo.vehicle_schedule_audit(schedule_id, created_at);
END
