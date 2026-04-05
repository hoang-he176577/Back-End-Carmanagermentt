USE [CarManager];
GO

SET NOCOUNT ON;
GO

/*
Seed realistic disposal insight data for the current database schema.
This script is idempotent and safe to run multiple times.
*/

DECLARE @Now DATETIME = GETDATE();

DECLARE @PrimaryBranchId INT;
DECLARE @SecondaryBranchId INT;
DECLARE @FallbackUserId INT;
DECLARE @OperatorUserId INT;
DECLARE @AccountantUserId INT;
DECLARE @ManagerUserId INT;

IF NOT EXISTS (SELECT 1 FROM dbo.role WHERE name = N'Operator')
BEGIN
    INSERT INTO dbo.role (name, description)
    VALUES (N'Operator', N'Seeded role for vehicle operation and disposal proposal creation');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.role WHERE name = N'Branch Asset Accountant')
BEGIN
    INSERT INTO dbo.role (name, description)
    VALUES (N'Branch Asset Accountant', N'Seeded role for branch asset accounting');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.role WHERE name = N'Executive Management')
BEGIN
    INSERT INTO dbo.role (name, description)
    VALUES (N'Executive Management', N'Seeded role for executive approval flows');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.role WHERE name = N'Manager')
BEGIN
    INSERT INTO dbo.role (name, description)
    VALUES (N'Manager', N'Seeded role for management access');
END;

SELECT TOP 1 @PrimaryBranchId = id
FROM dbo.branch
WHERE deleted_at IS NULL
ORDER BY CASE WHEN name LIKE N'%Hanoi%' THEN 0 WHEN name LIKE N'%HCMC%' THEN 1 ELSE 2 END, id;

SELECT TOP 1 @SecondaryBranchId = id
FROM dbo.branch
WHERE deleted_at IS NULL
  AND id <> ISNULL(@PrimaryBranchId, -1)
ORDER BY CASE WHEN name LIKE N'%Danang%' THEN 0 WHEN name LIKE N'%HCMC%' THEN 1 ELSE 2 END, id;

IF @PrimaryBranchId IS NULL
BEGIN
    INSERT INTO dbo.branch (name, address, created_at, updated_at)
    VALUES (N'Seed Branch 1', N'Seed Address 1', @Now, @Now);
    SET @PrimaryBranchId = SCOPE_IDENTITY();
END;

IF @SecondaryBranchId IS NULL
BEGIN
    INSERT INTO dbo.branch (name, address, created_at, updated_at)
    VALUES (N'Seed Branch 2', N'Seed Address 2', @Now, @Now);
    SET @SecondaryBranchId = SCOPE_IDENTITY();
END;

IF NOT EXISTS (SELECT 1 FROM dbo.[user] WHERE deleted_at IS NULL)
BEGIN
    DECLARE @OperatorRoleId INT = (SELECT TOP 1 id FROM dbo.role WHERE name = N'Operator' ORDER BY id);
    DECLARE @AccountantRoleId INT = (SELECT TOP 1 id FROM dbo.role WHERE name = N'Branch Asset Accountant' ORDER BY id);
    DECLARE @ExecutiveRoleId INT = (SELECT TOP 1 id FROM dbo.role WHERE name = N'Executive Management' ORDER BY id);

    INSERT INTO dbo.[user] (name, email, phone, branch_id, password_hash, email_verified, created_at, updated_at)
    VALUES (N'Seed Operator', N'seed.operator@carmanager.local', N'0901000001', @PrimaryBranchId, N'seed-hash', 1, @Now, @Now);
    SET @OperatorUserId = SCOPE_IDENTITY();

    INSERT INTO dbo.[user] (name, email, phone, branch_id, password_hash, email_verified, created_at, updated_at)
    VALUES (N'Seed Accountant', N'seed.accountant@carmanager.local', N'0901000002', @PrimaryBranchId, N'seed-hash', 1, @Now, @Now);
    SET @AccountantUserId = SCOPE_IDENTITY();

    INSERT INTO dbo.[user] (name, email, phone, branch_id, password_hash, email_verified, created_at, updated_at)
    VALUES (N'Seed Executive', N'seed.executive@carmanager.local', N'0901000003', @PrimaryBranchId, N'seed-hash', 1, @Now, @Now);
    SET @ManagerUserId = SCOPE_IDENTITY();

    IF @OperatorRoleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.user_role WHERE user_id = @OperatorUserId AND role_id = @OperatorRoleId)
        INSERT INTO dbo.user_role (user_id, role_id) VALUES (@OperatorUserId, @OperatorRoleId);

    IF @AccountantRoleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.user_role WHERE user_id = @AccountantUserId AND role_id = @AccountantRoleId)
        INSERT INTO dbo.user_role (user_id, role_id) VALUES (@AccountantUserId, @AccountantRoleId);

    IF @ExecutiveRoleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.user_role WHERE user_id = @ManagerUserId AND role_id = @ExecutiveRoleId)
        INSERT INTO dbo.user_role (user_id, role_id) VALUES (@ManagerUserId, @ExecutiveRoleId);
END;

SELECT TOP 1 @FallbackUserId = id
FROM dbo.[user]
WHERE deleted_at IS NULL
ORDER BY id;

SELECT TOP 1 @OperatorUserId = u.id
FROM dbo.[user] u
JOIN dbo.user_role ur ON ur.user_id = u.id
JOIN dbo.role r ON r.id = ur.role_id
WHERE u.deleted_at IS NULL
  AND r.name = N'Operator'
ORDER BY u.id;

SELECT TOP 1 @AccountantUserId = u.id
FROM dbo.[user] u
JOIN dbo.user_role ur ON ur.user_id = u.id
JOIN dbo.role r ON r.id = ur.role_id
WHERE u.deleted_at IS NULL
  AND r.name IN (N'Branch Asset Accountant', N'Executive Management', N'Manager')
ORDER BY CASE WHEN r.name = N'Branch Asset Accountant' THEN 0 ELSE 1 END, u.id;

SELECT TOP 1 @ManagerUserId = u.id
FROM dbo.[user] u
JOIN dbo.user_role ur ON ur.user_id = u.id
JOIN dbo.role r ON r.id = ur.role_id
WHERE u.deleted_at IS NULL
  AND r.name IN (N'Executive Management', N'Manager')
ORDER BY CASE WHEN r.name = N'Executive Management' THEN 0 ELSE 1 END, u.id;

SET @OperatorUserId = ISNULL(@OperatorUserId, @FallbackUserId);
SET @AccountantUserId = ISNULL(@AccountantUserId, ISNULL(@ManagerUserId, @FallbackUserId));
SET @ManagerUserId = ISNULL(@ManagerUserId, ISNULL(@AccountantUserId, @FallbackUserId));
SELECT @PrimaryBranchId = COALESCE((SELECT branch_id FROM dbo.[user] WHERE id = @OperatorUserId), @PrimaryBranchId);

DECLARE @EnsureOperatorRoleId INT = (SELECT TOP 1 id FROM dbo.role WHERE name = N'Operator' ORDER BY id);
DECLARE @EnsureAccountantRoleId INT = (SELECT TOP 1 id FROM dbo.role WHERE name = N'Branch Asset Accountant' ORDER BY id);
DECLARE @EnsureExecutiveRoleId INT = (SELECT TOP 1 id FROM dbo.role WHERE name = N'Executive Management' ORDER BY id);

IF @EnsureOperatorRoleId IS NOT NULL AND @OperatorUserId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.user_role WHERE user_id = @OperatorUserId AND role_id = @EnsureOperatorRoleId)
BEGIN
    INSERT INTO dbo.user_role (user_id, role_id) VALUES (@OperatorUserId, @EnsureOperatorRoleId);
END;

IF @EnsureAccountantRoleId IS NOT NULL AND @AccountantUserId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.user_role WHERE user_id = @AccountantUserId AND role_id = @EnsureAccountantRoleId)
BEGIN
    INSERT INTO dbo.user_role (user_id, role_id) VALUES (@AccountantUserId, @EnsureAccountantRoleId);
END;

IF @EnsureExecutiveRoleId IS NOT NULL AND @ManagerUserId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.user_role WHERE user_id = @ManagerUserId AND role_id = @EnsureExecutiveRoleId)
BEGIN
    INSERT INTO dbo.user_role (user_id, role_id) VALUES (@ManagerUserId, @EnsureExecutiveRoleId);
END;

DECLARE @InnovaModelId INT;
DECLARE @RangerModelId INT;
DECLARE @CountyModelId INT;

SELECT @InnovaModelId = id
FROM dbo.vehicle_model
WHERE manufacturer = N'Toyota' AND model_name = N'Innova 2.0E' AND deleted_at IS NULL;

IF @InnovaModelId IS NULL
BEGIN
    INSERT INTO dbo.vehicle_model
    (
        manufacturer, model_name, year_from, year_to, seats, engine_type, default_price,
        engine_power, emission_standard, payload_capacity, fuel_type, created_at, updated_at
    )
    VALUES
    (
        N'Toyota', N'Innova 2.0E', 2019, 2023, 8, N'Gasoline', 865000000,
        N'102 kW', N'Euro 4', 820, N'Gasoline', @Now, @Now
    );
    SET @InnovaModelId = SCOPE_IDENTITY();
END;

SELECT @RangerModelId = id
FROM dbo.vehicle_model
WHERE manufacturer = N'Ford' AND model_name = N'Ranger XLS 2.2 AT' AND deleted_at IS NULL;

IF @RangerModelId IS NULL
BEGIN
    INSERT INTO dbo.vehicle_model
    (
        manufacturer, model_name, year_from, year_to, seats, engine_type, default_price,
        engine_power, emission_standard, payload_capacity, fuel_type, created_at, updated_at
    )
    VALUES
    (
        N'Ford', N'Ranger XLS 2.2 AT', 2018, 2022, 5, N'Diesel', 780000000,
        N'118 kW', N'Euro 4', 950, N'Diesel', @Now, @Now
    );
    SET @RangerModelId = SCOPE_IDENTITY();
END;

SELECT @CountyModelId = id
FROM dbo.vehicle_model
WHERE manufacturer = N'Hyundai' AND model_name = N'County New Breeze' AND deleted_at IS NULL;

IF @CountyModelId IS NULL
BEGIN
    INSERT INTO dbo.vehicle_model
    (
        manufacturer, model_name, year_from, year_to, seats, engine_type, default_price,
        engine_power, emission_standard, payload_capacity, fuel_type, created_at, updated_at
    )
    VALUES
    (
        N'Hyundai', N'County New Breeze', 2016, 2021, 29, N'Diesel', 1120000000,
        N'103 kW', N'Euro 4', 1800, N'Diesel', @Now, @Now
    );
    SET @CountyModelId = SCOPE_IDENTITY();
END;

DECLARE @DriverMinhId INT;
DECLARE @DriverTuanId INT;
DECLARE @DriverPhongId INT;

SELECT @DriverMinhId = id FROM dbo.driver WHERE license_number = N'SEED-DISP-DRV-001' AND deleted_at IS NULL;
IF @DriverMinhId IS NULL
BEGIN
    INSERT INTO dbo.driver (name, license_number, phone, email, hire_date, status, branch_id, created_at, updated_at)
    VALUES (N'Nguyen Van Minh', N'SEED-DISP-DRV-001', N'0908123456', N'nguyenvanminh.seed@carmanager.local', '2021-03-15', N'Active', @PrimaryBranchId, @Now, @Now);
    SET @DriverMinhId = SCOPE_IDENTITY();
END;

SELECT @DriverTuanId = id FROM dbo.driver WHERE license_number = N'SEED-DISP-DRV-002' AND deleted_at IS NULL;
IF @DriverTuanId IS NULL
BEGIN
    INSERT INTO dbo.driver (name, license_number, phone, email, hire_date, status, branch_id, created_at, updated_at)
    VALUES (N'Le Anh Tuan', N'SEED-DISP-DRV-002', N'0909345678', N'leanhtuan.seed@carmanager.local', '2020-08-01', N'Active', @PrimaryBranchId, @Now, @Now);
    SET @DriverTuanId = SCOPE_IDENTITY();
END;

SELECT @DriverPhongId = id FROM dbo.driver WHERE license_number = N'SEED-DISP-DRV-003' AND deleted_at IS NULL;
IF @DriverPhongId IS NULL
BEGIN
    INSERT INTO dbo.driver (name, license_number, phone, email, hire_date, status, branch_id, created_at, updated_at)
    VALUES (N'Tran Quoc Phong', N'SEED-DISP-DRV-003', N'0911223344', N'tranquocphong.seed@carmanager.local', '2019-06-20', N'Active', @PrimaryBranchId, @Now, @Now);
    SET @DriverPhongId = SCOPE_IDENTITY();
END;

DECLARE @VehiclePendingId INT;
DECLARE @VehicleRejectedId INT;
DECLARE @VehicleApprovedId INT;

SELECT @VehiclePendingId = id FROM dbo.vehicle WHERE license_plate = N'51A-889.90' AND deleted_at IS NULL;
IF @VehiclePendingId IS NULL
BEGIN
    INSERT INTO dbo.vehicle
    (
        license_plate, model_id, year_manufacture, purchase_date, original_cost, current_value, mileage,
        status, current_branch_id, current_driver_id, image_url, vin, engine_number, chassis_number,
        registration_expiration_date, insurance_expiration_date, badge_type, badge_expiration_date, fuel_norm,
        created_at, updated_at
    )
    VALUES
    (
        N'51A-889.90', @InnovaModelId, 2020, '2020-05-18', 865000000, 212000000, 184560,
        N'Active', @PrimaryBranchId, @DriverMinhId, N'/uploads/vehicles/seed-51A-889-90.jpg', N'RL4MG12A0LH000901', N'ENG-INNOVA-000901', N'CHS-INNOVA-000901',
        '2026-11-30', '2026-09-15', N'Xe hop dong', '2026-12-31', 11.20, @Now, @Now
    );
    SET @VehiclePendingId = SCOPE_IDENTITY();
END;

SELECT @VehicleRejectedId = id FROM dbo.vehicle WHERE license_plate = N'29A-678.45' AND deleted_at IS NULL;
IF @VehicleRejectedId IS NULL
BEGIN
    INSERT INTO dbo.vehicle
    (
        license_plate, model_id, year_manufacture, purchase_date, original_cost, current_value, mileage,
        status, current_branch_id, current_driver_id, image_url, vin, engine_number, chassis_number,
        registration_expiration_date, insurance_expiration_date, badge_type, badge_expiration_date, fuel_norm,
        created_at, updated_at
    )
    VALUES
    (
        N'29A-678.45', @RangerModelId, 2019, '2019-09-03', 780000000, 298000000, 142300,
        N'Active', @PrimaryBranchId, @DriverTuanId, N'/uploads/vehicles/seed-29A-678-45.jpg', N'MNAUMFF50KW123678', N'ENG-RANGER-123678', N'CHS-RANGER-123678',
        '2026-08-20', '2026-08-20', N'Xe cong vu', '2026-10-10', 9.50, @Now, @Now
    );
    SET @VehicleRejectedId = SCOPE_IDENTITY();
END;

SELECT @VehicleApprovedId = id FROM dbo.vehicle WHERE license_plate = N'43B-228.17' AND deleted_at IS NULL;
IF @VehicleApprovedId IS NULL
BEGIN
    INSERT INTO dbo.vehicle
    (
        license_plate, model_id, year_manufacture, purchase_date, original_cost, current_value, mileage,
        status, current_branch_id, current_driver_id, image_url, vin, engine_number, chassis_number,
        registration_expiration_date, insurance_expiration_date, badge_type, badge_expiration_date, fuel_norm,
        created_at, updated_at
    )
    VALUES
    (
        N'43B-228.17', @CountyModelId, 2017, '2017-04-11', 1120000000, 165000000, 286450,
        N'Disposed', @PrimaryBranchId, NULL, N'/uploads/vehicles/seed-43B-228-17.jpg', N'KMJHD17HPHU004317', N'ENG-COUNTY-004317', N'CHS-COUNTY-004317',
        '2025-12-31', '2025-12-31', N'Xe hop dong', '2025-12-31', 16.80, @Now, @Now
    );
    SET @VehicleApprovedId = SCOPE_IDENTITY();
END;

UPDATE dbo.vehicle
SET current_branch_id = @PrimaryBranchId,
    current_driver_id = @DriverMinhId,
    status = N'Active',
    current_value = 212000000,
    mileage = 184560,
    updated_at = @Now
WHERE id = @VehiclePendingId;

UPDATE dbo.vehicle
SET current_branch_id = @PrimaryBranchId,
    current_driver_id = @DriverTuanId,
    status = N'Active',
    current_value = 298000000,
    mileage = 142300,
    updated_at = @Now
WHERE id = @VehicleRejectedId;

UPDATE dbo.vehicle
SET current_branch_id = @PrimaryBranchId,
    current_driver_id = NULL,
    status = N'Disposed',
    current_value = 165000000,
    mileage = 286450,
    updated_at = @Now
WHERE id = @VehicleApprovedId;

/* Transfer plans */
DECLARE @PlanPending1 INT, @PlanPending2 INT, @PlanPending3 INT;
DECLARE @PlanRejected1 INT, @PlanRejected2 INT;
DECLARE @PlanApproved1 INT, @PlanApproved2 INT;

SELECT @PlanPending1 = id FROM dbo.transfer_plan WHERE vehicle_id = @VehiclePendingId AND plan_date = '2025-03-12' AND deleted_at IS NULL;
IF @PlanPending1 IS NULL
BEGIN
    INSERT INTO dbo.transfer_plan (vehicle_id, from_branch_id, to_branch_id, manager_id, plan_date, executed_date, status, created_at, updated_at, checkout_date, checkout_by_user_id, checkin_date, checkin_by_user_id, planned_departure_date, planned_arrival_date, checkout_note, checkin_note)
    VALUES (@VehiclePendingId, @PrimaryBranchId, @SecondaryBranchId, @ManagerUserId, '2025-03-12', '2025-03-12', N'Completed', @Now, @Now, '2025-03-12T06:35:00', @OperatorUserId, '2025-03-12T14:05:00', @OperatorUserId, '2025-03-12T06:30:00', '2025-03-12T14:30:00', N'Seed disposal trip TP-HCM to Da Nang for branch delivery.', N'Seed disposal trip completed on schedule.');
    SET @PlanPending1 = SCOPE_IDENTITY();
END;

SELECT @PlanPending2 = id FROM dbo.transfer_plan WHERE vehicle_id = @VehiclePendingId AND plan_date = '2025-06-18' AND deleted_at IS NULL;
IF @PlanPending2 IS NULL
BEGIN
    INSERT INTO dbo.transfer_plan (vehicle_id, from_branch_id, to_branch_id, manager_id, plan_date, executed_date, status, created_at, updated_at, checkout_date, checkout_by_user_id, checkin_date, checkin_by_user_id, planned_departure_date, planned_arrival_date, checkout_note, checkin_note)
    VALUES (@VehiclePendingId, @SecondaryBranchId, @PrimaryBranchId, @ManagerUserId, '2025-06-18', '2025-06-18', N'Completed', @Now, @Now, '2025-06-18T07:20:00', @OperatorUserId, '2025-06-18T14:50:00', @OperatorUserId, '2025-06-18T07:15:00', '2025-06-18T15:10:00', N'Seed return trip after equipment handover.', N'Returned to base with minor vibration noted on front axle.');
    SET @PlanPending2 = SCOPE_IDENTITY();
END;

SELECT @PlanPending3 = id FROM dbo.transfer_plan WHERE vehicle_id = @VehiclePendingId AND plan_date = '2025-11-07' AND deleted_at IS NULL;
IF @PlanPending3 IS NULL
BEGIN
    INSERT INTO dbo.transfer_plan (vehicle_id, from_branch_id, to_branch_id, manager_id, plan_date, executed_date, status, created_at, updated_at, checkout_date, checkout_by_user_id, checkin_date, checkin_by_user_id, planned_departure_date, planned_arrival_date, checkout_note, checkin_note)
    VALUES (@VehiclePendingId, @PrimaryBranchId, @SecondaryBranchId, @ManagerUserId, '2025-11-07', '2025-11-07', N'Completed', @Now, @Now, '2025-11-07T05:50:00', @OperatorUserId, '2025-11-07T13:42:00', @OperatorUserId, '2025-11-07T05:45:00', '2025-11-07T13:50:00', N'Seed shuttle trip for regional audit support.', N'Trip completed; steering felt heavy on mountain section.');
    SET @PlanPending3 = SCOPE_IDENTITY();
END;

SELECT @PlanRejected1 = id FROM dbo.transfer_plan WHERE vehicle_id = @VehicleRejectedId AND plan_date = '2025-04-22' AND deleted_at IS NULL;
IF @PlanRejected1 IS NULL
BEGIN
    INSERT INTO dbo.transfer_plan (vehicle_id, from_branch_id, to_branch_id, manager_id, plan_date, executed_date, status, created_at, updated_at, checkout_date, checkout_by_user_id, checkin_date, checkin_by_user_id, planned_departure_date, planned_arrival_date, checkout_note, checkin_note)
    VALUES (@VehicleRejectedId, @PrimaryBranchId, @SecondaryBranchId, @ManagerUserId, '2025-04-22', '2025-04-22', N'Completed', @Now, @Now, '2025-04-22T08:05:00', @OperatorUserId, '2025-04-22T13:40:00', @OperatorUserId, '2025-04-22T08:00:00', '2025-04-22T14:00:00', N'Seed field inspection trip for construction site supervision.', N'Returned with no major mechanical issue.');
    SET @PlanRejected1 = SCOPE_IDENTITY();
END;

SELECT @PlanRejected2 = id FROM dbo.transfer_plan WHERE vehicle_id = @VehicleRejectedId AND plan_date = '2025-12-03' AND deleted_at IS NULL;
IF @PlanRejected2 IS NULL
BEGIN
    INSERT INTO dbo.transfer_plan (vehicle_id, from_branch_id, to_branch_id, manager_id, plan_date, executed_date, status, created_at, updated_at, checkout_date, checkout_by_user_id, checkin_date, checkin_by_user_id, planned_departure_date, planned_arrival_date, checkout_note, checkin_note)
    VALUES (@VehicleRejectedId, @SecondaryBranchId, @PrimaryBranchId, @ManagerUserId, '2025-12-03', '2025-12-03', N'Completed', @Now, @Now, '2025-12-03T07:30:00', @OperatorUserId, '2025-12-03T13:08:00', @OperatorUserId, '2025-12-03T07:25:00', '2025-12-03T13:20:00', N'Seed return trip after site survey.', N'Observed rear suspension noise when empty load.');
    SET @PlanRejected2 = SCOPE_IDENTITY();
END;

SELECT @PlanApproved1 = id FROM dbo.transfer_plan WHERE vehicle_id = @VehicleApprovedId AND plan_date = '2025-01-17' AND deleted_at IS NULL;
IF @PlanApproved1 IS NULL
BEGIN
    INSERT INTO dbo.transfer_plan (vehicle_id, from_branch_id, to_branch_id, manager_id, plan_date, executed_date, status, created_at, updated_at, checkout_date, checkout_by_user_id, checkin_date, checkin_by_user_id, planned_departure_date, planned_arrival_date, checkout_note, checkin_note)
    VALUES (@VehicleApprovedId, @PrimaryBranchId, @SecondaryBranchId, @ManagerUserId, '2025-01-17', '2025-01-17', N'Completed', @Now, @Now, '2025-01-17T05:40:00', @OperatorUserId, '2025-01-17T17:18:00', @OperatorUserId, '2025-01-17T05:30:00', '2025-01-17T17:30:00', N'Seed intercity shuttle trip for staff transport.', N'Vehicle returned with high coolant temperature warning.');
    SET @PlanApproved1 = SCOPE_IDENTITY();
END;

SELECT @PlanApproved2 = id FROM dbo.transfer_plan WHERE vehicle_id = @VehicleApprovedId AND plan_date = '2025-08-09' AND deleted_at IS NULL;
IF @PlanApproved2 IS NULL
BEGIN
    INSERT INTO dbo.transfer_plan (vehicle_id, from_branch_id, to_branch_id, manager_id, plan_date, executed_date, status, created_at, updated_at, checkout_date, checkout_by_user_id, checkin_date, checkin_by_user_id, planned_departure_date, planned_arrival_date, checkout_note, checkin_note)
    VALUES (@VehicleApprovedId, @SecondaryBranchId, @PrimaryBranchId, @ManagerUserId, '2025-08-09', '2025-08-09', N'Completed', @Now, @Now, '2025-08-09T06:15:00', @OperatorUserId, '2025-08-09T18:12:00', @OperatorUserId, '2025-08-09T06:10:00', '2025-08-09T18:30:00', N'Seed return trip after event support deployment.', N'Heavy smoke observed under load near final 40 km.');
    SET @PlanApproved2 = SCOPE_IDENTITY();
END;

/* Trip logs */
IF NOT EXISTS (SELECT 1 FROM dbo.trip_log WHERE transfer_plan_id = @PlanPending1 AND start_time = '2025-03-12T06:35:00')
BEGIN
    INSERT INTO dbo.trip_log (vehicle_id, driver_id, transfer_plan_id, start_time, end_time, start_mileage, end_mileage, origin, destination, started_by, ended_by, created_at)
    VALUES (@VehiclePendingId, @DriverMinhId, @PlanPending1, '2025-03-12T06:35:00', '2025-03-12T14:05:00', 136240, 136985, N'Ha Noi', N'Da Nang', @OperatorUserId, @OperatorUserId, @Now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.trip_log WHERE transfer_plan_id = @PlanPending2 AND start_time = '2025-06-18T07:20:00')
BEGIN
    INSERT INTO dbo.trip_log (vehicle_id, driver_id, transfer_plan_id, start_time, end_time, start_mileage, end_mileage, origin, destination, started_by, ended_by, created_at)
    VALUES (@VehiclePendingId, @DriverMinhId, @PlanPending2, '2025-06-18T07:20:00', '2025-06-18T14:50:00', 148930, 149712, N'Da Nang', N'Ha Noi', @OperatorUserId, @OperatorUserId, @Now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.trip_log WHERE transfer_plan_id = @PlanPending3 AND start_time = '2025-11-07T05:50:00')
BEGIN
    INSERT INTO dbo.trip_log (vehicle_id, driver_id, transfer_plan_id, start_time, end_time, start_mileage, end_mileage, origin, destination, started_by, ended_by, created_at)
    VALUES (@VehiclePendingId, @DriverMinhId, @PlanPending3, '2025-11-07T05:50:00', '2025-11-07T13:42:00', 162115, 162908, N'Ha Noi', N'Da Nang', @OperatorUserId, @OperatorUserId, @Now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.trip_log WHERE transfer_plan_id = @PlanRejected1 AND start_time = '2025-04-22T08:05:00')
BEGIN
    INSERT INTO dbo.trip_log (vehicle_id, driver_id, transfer_plan_id, start_time, end_time, start_mileage, end_mileage, origin, destination, started_by, ended_by, created_at)
    VALUES (@VehicleRejectedId, @DriverTuanId, @PlanRejected1, '2025-04-22T08:05:00', '2025-04-22T13:40:00', 121540, 121998, N'Ha Noi', N'Da Nang', @OperatorUserId, @OperatorUserId, @Now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.trip_log WHERE transfer_plan_id = @PlanRejected2 AND start_time = '2025-12-03T07:30:00')
BEGIN
    INSERT INTO dbo.trip_log (vehicle_id, driver_id, transfer_plan_id, start_time, end_time, start_mileage, end_mileage, origin, destination, started_by, ended_by, created_at)
    VALUES (@VehicleRejectedId, @DriverTuanId, @PlanRejected2, '2025-12-03T07:30:00', '2025-12-03T13:08:00', 139880, 140366, N'Da Nang', N'Ha Noi', @OperatorUserId, @OperatorUserId, @Now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.trip_log WHERE transfer_plan_id = @PlanApproved1 AND start_time = '2025-01-17T05:40:00')
BEGIN
    INSERT INTO dbo.trip_log (vehicle_id, driver_id, transfer_plan_id, start_time, end_time, start_mileage, end_mileage, origin, destination, started_by, ended_by, created_at)
    VALUES (@VehicleApprovedId, @DriverPhongId, @PlanApproved1, '2025-01-17T05:40:00', '2025-01-17T17:18:00', 271420, 272265, N'Ha Noi', N'Da Nang', @OperatorUserId, @OperatorUserId, @Now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.trip_log WHERE transfer_plan_id = @PlanApproved2 AND start_time = '2025-08-09T06:15:00')
BEGIN
    INSERT INTO dbo.trip_log (vehicle_id, driver_id, transfer_plan_id, start_time, end_time, start_mileage, end_mileage, origin, destination, started_by, ended_by, created_at)
    VALUES (@VehicleApprovedId, @DriverPhongId, @PlanApproved2, '2025-08-09T06:15:00', '2025-08-09T18:12:00', 284930, 286450, N'Da Nang', N'Ha Noi', @OperatorUserId, @OperatorUserId, @Now);
END;

/* Maintenance history */
IF NOT EXISTS (SELECT 1 FROM dbo.maintenance_request WHERE vehicle_id = @VehiclePendingId AND request_date = '2025-02-20')
BEGIN
    INSERT INTO dbo.maintenance_request
    (
        vehicle_id, operator_id, request_date, description, estimated_cost, status,
        maintenance_type, accountant_id, approved_date, actual_cost, completion_date, completion_note, approval_note,
        created_at, updated_at
    )
    VALUES
    (
        @VehiclePendingId, @OperatorUserId, '2025-02-20',
        N'Periodic maintenance at 135,000 km: engine oil, oil filter, air filter, brake fluid check.',
        4800000, N'Completed',
        N'Periodic', @AccountantUserId, '2025-02-21', 4620000, '2025-02-22', N'Completed at authorized garage. Front brake pads still usable for next cycle.', N'Approved within standard maintenance budget.',
        @Now, @Now
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.maintenance_request WHERE vehicle_id = @VehiclePendingId AND request_date = '2025-07-01')
BEGIN
    INSERT INTO dbo.maintenance_request
    (
        vehicle_id, operator_id, request_date, description, estimated_cost, status,
        maintenance_type, accountant_id, approved_date, actual_cost, completion_date, completion_note, approval_note,
        created_at, updated_at
    )
    VALUES
    (
        @VehiclePendingId, @OperatorUserId, '2025-07-01',
        N'Repair front suspension after repeated vibration on highway and uneven tire wear.',
        18600000, N'Completed',
        N'Repair', @AccountantUserId, '2025-07-02', 19350000, '2025-07-05', N'Replaced lower arm bushings, front shock absorbers, wheel alignment and balancing completed.', N'Approved due to direct impact on driving safety.',
        @Now, @Now
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.maintenance_request WHERE vehicle_id = @VehiclePendingId AND request_date = '2025-12-16')
BEGIN
    INSERT INTO dbo.maintenance_request
    (
        vehicle_id, operator_id, request_date, description, estimated_cost, status,
        maintenance_type, accountant_id, approved_date, actual_cost, completion_date, completion_note, approval_note,
        created_at, updated_at
    )
    VALUES
    (
        @VehiclePendingId, @OperatorUserId, '2025-12-16',
        N'Engine check light on, rough idle, weak acceleration when carrying 6 to 7 passengers.',
        22700000, N'Completed',
        N'Breakdown', @AccountantUserId, '2025-12-16', 24180000, '2025-12-20', N'Cleaned throttle body, replaced ignition coil set and fuel pump assembly, reset ECU fault codes.', N'Approved because vehicle was unavailable for branch operation.',
        @Now, @Now
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.maintenance_request WHERE vehicle_id = @VehiclePendingId AND request_date = '2026-03-10')
BEGIN
    INSERT INTO dbo.maintenance_request
    (
        vehicle_id, operator_id, request_date, description, estimated_cost, status,
        maintenance_type, accountant_id, approved_date, actual_cost, completion_date, completion_note, approval_note,
        created_at, updated_at
    )
    VALUES
    (
        @VehiclePendingId, @OperatorUserId, '2026-03-10',
        N'Persistent oil consumption, steering rack noise and body vibration at 80 km/h and above.',
        36500000, N'Approved',
        N'Repair', @AccountantUserId, '2026-03-11', NULL, NULL, NULL, N'Approved for diagnostic teardown. Vehicle considered candidate for disposal after cost review.',
        @Now, @Now
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.maintenance_request WHERE vehicle_id = @VehicleRejectedId AND request_date = '2025-05-06')
BEGIN
    INSERT INTO dbo.maintenance_request
    (
        vehicle_id, operator_id, request_date, description, estimated_cost, status,
        maintenance_type, accountant_id, approved_date, actual_cost, completion_date, completion_note, approval_note,
        created_at, updated_at
    )
    VALUES
    (
        @VehicleRejectedId, @OperatorUserId, '2025-05-06',
        N'Periodic maintenance at 120,000 km: fluids, transmission oil check, rear brake inspection.',
        6200000, N'Completed',
        N'Periodic', @AccountantUserId, '2025-05-07', 5980000, '2025-05-08', N'Periodic maintenance completed. Rear brake cylinder cleaned and lubricated.', N'Approved under routine branch operating budget.',
        @Now, @Now
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.maintenance_request WHERE vehicle_id = @VehicleRejectedId AND request_date = '2025-10-14')
BEGIN
    INSERT INTO dbo.maintenance_request
    (
        vehicle_id, operator_id, request_date, description, estimated_cost, status,
        maintenance_type, accountant_id, approved_date, actual_cost, completion_date, completion_note, approval_note,
        created_at, updated_at
    )
    VALUES
    (
        @VehicleRejectedId, @OperatorUserId, '2025-10-14',
        N'Rear suspension clunking under empty load, minor oil leak at front differential housing.',
        28900000, N'Completed',
        N'Breakdown', @AccountantUserId, '2025-10-15', 27400000, '2025-10-19', N'Replaced rear leaf spring bushings, sealed differential housing and performed road test.', N'Approved because asset remained operational and repair cost still lower than disposal threshold.',
        @Now, @Now
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.maintenance_request WHERE vehicle_id = @VehicleApprovedId AND request_date = '2024-11-05')
BEGIN
    INSERT INTO dbo.maintenance_request
    (
        vehicle_id, operator_id, request_date, description, estimated_cost, status,
        maintenance_type, accountant_id, approved_date, actual_cost, completion_date, completion_note, approval_note,
        created_at, updated_at
    )
    VALUES
    (
        @VehicleApprovedId, @OperatorUserId, '2024-11-05',
        N'Full periodic maintenance for 270,000 km bus operation cycle, including brake and cooling system check.',
        16500000, N'Completed',
        N'Periodic', @AccountantUserId, '2024-11-06', 17150000, '2024-11-09', N'Cooling hose and brake vacuum line replaced during preventive maintenance.', N'Approved because vehicle was still in active shuttle rotation.',
        @Now, @Now
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.maintenance_request WHERE vehicle_id = @VehicleApprovedId AND request_date = '2025-06-28')
BEGIN
    INSERT INTO dbo.maintenance_request
    (
        vehicle_id, operator_id, request_date, description, estimated_cost, status,
        maintenance_type, accountant_id, approved_date, actual_cost, completion_date, completion_note, approval_note,
        created_at, updated_at
    )
    VALUES
    (
        @VehicleApprovedId, @OperatorUserId, '2025-06-28',
        N'Overheating on long route, white smoke at cold start, gearbox shift delay and heavy fuel consumption.',
        86400000, N'Completed',
        N'Breakdown', @AccountantUserId, '2025-06-28', 91250000, '2025-07-06', N'Cylinder head gasket, water pump, radiator core and gearbox valve body repaired. Vehicle returned but reliability remained low.', N'Approved because failure caused service interruption for 2 operating days.',
        @Now, @Now
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.maintenance_request WHERE vehicle_id = @VehicleApprovedId AND request_date = '2025-09-18')
BEGIN
    INSERT INTO dbo.maintenance_request
    (
        vehicle_id, operator_id, request_date, description, estimated_cost, status,
        maintenance_type, accountant_id, approved_date, actual_cost, completion_date, completion_note, approval_note,
        created_at, updated_at
    )
    VALUES
    (
        @VehicleApprovedId, @OperatorUserId, '2025-09-18',
        N'Post-repair follow-up: turbo lag, exhaust smoke under load and cabin floor vibration continue after major overhaul.',
        53700000, N'Completed',
        N'Repair', @AccountantUserId, '2025-09-19', 55800000, '2025-09-24', N'Turbocharger cleaned, engine mounts replaced, exhaust line partially renewed. Reliability still not acceptable for passenger transport.', N'Approved as last repair attempt before recommending disposal.',
        @Now, @Now
    );
END;

/* Disposal proposals */
IF NOT EXISTS (SELECT 1 FROM dbo.disposal_proposal WHERE vehicle_id = @VehiclePendingId AND created_date = '2026-03-25' AND status = N'Pending')
BEGIN
    INSERT INTO dbo.disposal_proposal (vehicle_id, proposer_id, manager_id, created_date, approved_date, status, proposed_price, reason, created_at, updated_at)
    VALUES (@VehiclePendingId, @OperatorUserId, @ManagerUserId, '2026-03-25', NULL, N'Pending', 185000000, N'Vehicle has high mileage, repeated suspension and engine repairs over the last 12 months, and repair estimate is approaching remaining book value.', @Now, @Now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.disposal_proposal WHERE vehicle_id = @VehicleRejectedId AND created_date = '2025-12-18' AND status = N'Rejected')
BEGIN
    INSERT INTO dbo.disposal_proposal (vehicle_id, proposer_id, manager_id, created_date, approved_date, status, proposed_price, reason, created_at, updated_at)
    VALUES (@VehicleRejectedId, @OperatorUserId, @ManagerUserId, '2025-12-18', NULL, N'Rejected', 255000000, N'Proposal submitted after suspension and drivetrain complaints, but management rejected because the vehicle remained useful for medium duty branch operations after repair.', @Now, @Now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.disposal_proposal WHERE vehicle_id = @VehicleApprovedId AND created_date = '2025-10-02' AND status = N'Approved')
BEGIN
    INSERT INTO dbo.disposal_proposal (vehicle_id, proposer_id, manager_id, created_date, approved_date, status, proposed_price, reason, created_at, updated_at)
    VALUES (@VehicleApprovedId, @OperatorUserId, @ManagerUserId, '2025-10-02', '2025-10-07', N'Approved', 160000000, N'Vehicle recorded repeated overheating, smoke and drivetrain issues despite multiple high cost repairs. Disposal approved to avoid further operating risk.', @Now, @Now);
END;

PRINT 'Seed completed for disposal insight test data.';
PRINT 'Pending: 51A-889.90 | Rejected: 29A-678.45 | Approved: 43B-228.17';
GO
