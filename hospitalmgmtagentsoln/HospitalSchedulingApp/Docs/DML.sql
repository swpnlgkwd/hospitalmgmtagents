
INSERT INTO Department (department_name)
VALUES 
    ('ICU'),
    ('Pediatrics'),
    ('General'),
    ('Emergency');
Go;

INSERT INTO [dbo].[LeaveTypes] (leave_type_name) VALUES ('Sick'), ('Casual'), ('Vacation');
GO;

INSERT INTO LeaveStatus (leave_status_name)
VALUES 
    ('Pending'),
    ('Approved'),
    ('Rejected');
Go;

INSERT INTO Role (role_name)
VALUES 
    ('Employee'),
    ('Scheduler');
Go;

INSERT INTO ShiftStatus (shift_status_name) VALUES
('Scheduled'),
('Assigned'),
('Completed'),
('Cancelled');
INSERT INTO ShiftStatus (shift_status_name) Values ('Vacant')
Go;

INSERT INTO ShiftType (shift_type_name, start_time, end_time) VALUES
('Night', '00:00:00', '08:00:00'),
('Morning', '08:00:00', '16:00:00'),
('Evening', '16:00:00', '23:59:59');
GO;
--  Scheduler
INSERT INTO Staff (staff_name, role_id, staff_department_id, is_active)
VALUES ('Elena Rodriguez', 2, 1, 1);  -- Scheduler (RoleId 2), department arbitrarily ICU

--  Employee
INSERT INTO Staff (staff_name, role_id, staff_department_id, is_active)
VALUES
('Olivia Thompson', 1, 1, 1),   -- ICU
('Liam Anderson',   1, 2, 1),   -- Pediatrics
('Emma Johnson',    1, 3, 1),   -- General
('Noah Martinez',   1, 4, 1),   -- Cardiology
('Ava Patel',       1, 1, 1),   -- ICU
('Mia Chen',        1, 3, 1);   -- General


-- Emma Johnson (General - department_id = 3)
INSERT INTO PlannedShift (shift_date, shift_type_id, department_id, slot_number, shift_status_id, assigned_staff_id)
VALUES 
('2025-08-01', 1, 3, 1, 1, 4),
('2025-08-02', 2, 3, 1, 1, 4),
('2025-08-03', 3, 3, 1, 1, 4);

-- Olivia Brown (General - department_id = 3)
INSERT INTO PlannedShift (shift_date, shift_type_id, department_id, slot_number, shift_status_id, assigned_staff_id)
VALUES 
('2025-08-01', 2, 3, 2, 1, 5),
('2025-08-02', 3, 3, 2, 1, 5),
('2025-08-03', 1, 3, 2, 1, 5);

-- Ava Davis (ICU - department_id = 1)
INSERT INTO PlannedShift (shift_date, shift_type_id, department_id, slot_number, shift_status_id, assigned_staff_id)
VALUES 
('2025-08-01', 1, 1, 1, 1, 6),
('2025-08-02', 2, 1, 1, 1, 6),
('2025-08-03', 3, 1, 1, 1, 6);




