SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Table: AgentConversations
-- =============================================
CREATE TABLE [dbo].[AgentConversations] (
    [user_id] NVARCHAR(50) NOT NULL,
    [thread_id] NVARCHAR(100) NULL,
    [created_at] DATETIME NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_AgentConversations] PRIMARY KEY CLUSTERED ([user_id] ASC)
) ON [PRIMARY];
GO

-- =============================================
-- Table: Department
-- =============================================
CREATE TABLE [dbo].[Department] (
    [department_id] INT IDENTITY(1,1) NOT NULL,
    [department_name] VARCHAR(100) NOT NULL,
    CONSTRAINT [PK_Department] PRIMARY KEY CLUSTERED ([department_id] ASC),
    CONSTRAINT [UQ_Department_Name] UNIQUE NONCLUSTERED ([department_name] ASC)
) ON [PRIMARY];
GO

-- =============================================
-- Table: Role
-- =============================================
CREATE TABLE [dbo].[Role] (
    [role_id] INT IDENTITY(1,1) NOT NULL,
    [role_name] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED ([role_id] ASC),
    CONSTRAINT [UQ_Role_Name] UNIQUE NONCLUSTERED ([role_name] ASC)
) ON [PRIMARY];
GO

-- =============================================
-- Table: ShiftStatus
-- =============================================
CREATE TABLE [dbo].[ShiftStatus] (
    [shift_status_id] INT IDENTITY(1,1) NOT NULL,
    [shift_status_name] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_ShiftStatus] PRIMARY KEY CLUSTERED ([shift_status_id] ASC),
    CONSTRAINT [UQ_ShiftStatus_Name] UNIQUE NONCLUSTERED ([shift_status_name] ASC)
) ON [PRIMARY];
GO

-- =============================================
-- Table: ShiftType
-- =============================================
CREATE TABLE [dbo].[ShiftType] (
    [shift_type_id] INT IDENTITY(1,1) NOT NULL,
    [shift_type_name] VARCHAR(50) NOT NULL,
    [start_time] TIME(7) NOT NULL,
    [end_time] TIME(7) NOT NULL,
    CONSTRAINT [PK_ShiftType] PRIMARY KEY CLUSTERED ([shift_type_id] ASC),
    CONSTRAINT [UQ_ShiftType_Name] UNIQUE NONCLUSTERED ([shift_type_name] ASC)
) ON [PRIMARY];
GO

-- =============================================
-- Table: ShiftPlanning
-- =============================================
CREATE TABLE [dbo].[ShiftPlanning] (
    [shift_date] DATE NOT NULL,
    [shift_type_id] INT NOT NULL,
    [department_id] INT NOT NULL,
    [required_nurses] INT NOT NULL CHECK ([required_nurses] >= 0),
    CONSTRAINT [PK_ShiftPlanning] PRIMARY KEY CLUSTERED ([shift_date], [shift_type_id], [department_id])
) ON [PRIMARY];
GO

ALTER TABLE [dbo].[ShiftPlanning] WITH CHECK ADD CONSTRAINT [FK_ShiftPlanning_Department] FOREIGN KEY([department_id]) REFERENCES [dbo].[Department]([department_id]);
ALTER TABLE [dbo].[ShiftPlanning] WITH CHECK ADD CONSTRAINT [FK_ShiftPlanning_ShiftType] FOREIGN KEY([shift_type_id]) REFERENCES [dbo].[ShiftType]([shift_type_id]);
GO

-- =============================================
-- Table: Staff
-- =============================================
CREATE TABLE [dbo].[Staff] (
    [staff_id] INT IDENTITY(1,1) NOT NULL,
    [staff_name] NVARCHAR(100) NOT NULL,
    [role_id] INT NOT NULL,
    [staff_department_id] INT NOT NULL,
    [is_active] BIT NOT NULL DEFAULT (1),
    CONSTRAINT [PK_Staff] PRIMARY KEY CLUSTERED ([staff_id] ASC)
) ON [PRIMARY];
GO

ALTER TABLE [dbo].[Staff] WITH CHECK ADD CONSTRAINT [FK_Staff_Department] FOREIGN KEY([staff_department_id]) REFERENCES [dbo].[Department]([department_id]);
ALTER TABLE [dbo].[Staff] WITH CHECK ADD CONSTRAINT [FK_Staff_Role] FOREIGN KEY([role_id]) REFERENCES [dbo].[Role]([role_id]);
GO

-- =============================================
-- Table: UserCredential
-- =============================================
CREATE TABLE [dbo].[UserCredential] (
    [credential_id] INT IDENTITY(1,1) NOT NULL,
    [staff_id] INT NOT NULL,
    [username] NVARCHAR(100) NOT NULL,
    [password_hash] NVARCHAR(256) NOT NULL,
    CONSTRAINT [PK_UserCredential] PRIMARY KEY CLUSTERED ([credential_id] ASC),
    CONSTRAINT [UQ_UserCredential_StaffId] UNIQUE NONCLUSTERED ([staff_id]),
    CONSTRAINT [UQ_UserCredential_Username] UNIQUE NONCLUSTERED ([username])
) ON [PRIMARY];
GO

ALTER TABLE [dbo].[UserCredential] WITH CHECK ADD CONSTRAINT [FK_UserCredential_Staff] FOREIGN KEY([staff_id]) REFERENCES [dbo].[Staff]([staff_id]);
GO

-- =============================================
-- Table: PlannedShift
-- =============================================
CREATE TABLE [dbo].[PlannedShift] (
    [planned_shift_id] INT IDENTITY(1,1) NOT NULL,
    [shift_date] DATE NOT NULL,
    [shift_type_id] INT NOT NULL,
    [department_id] INT NOT NULL,
    [slot_number] INT NOT NULL,
    [shift_status_id] INT NOT NULL,
    [assigned_staff_id] INT NULL,
    CONSTRAINT [PK_PlannedShift] PRIMARY KEY CLUSTERED ([planned_shift_id] ASC)
) ON [PRIMARY];
GO

ALTER TABLE [dbo].[PlannedShift] WITH CHECK ADD CONSTRAINT [FK_PlannedShift_Department] FOREIGN KEY([department_id]) REFERENCES [dbo].[Department]([department_id]);
ALTER TABLE [dbo].[PlannedShift] WITH CHECK ADD CONSTRAINT [FK_PlannedShift_ShiftType] FOREIGN KEY([shift_type_id]) REFERENCES [dbo].[ShiftType]([shift_type_id]);
ALTER TABLE [dbo].[PlannedShift] WITH CHECK ADD CONSTRAINT [FK_PlannedShift_Staff] FOREIGN KEY([assigned_staff_id]) REFERENCES [dbo].[Staff]([staff_id]);
ALTER TABLE [dbo].[PlannedShift] WITH CHECK ADD CONSTRAINT [FK_PlannedShift_Status] FOREIGN KEY([shift_status_id]) REFERENCES [dbo].[ShiftStatus]([shift_status_id]);
GO

-- =============================================
-- Table: NurseAvailability
-- =============================================
CREATE TABLE [dbo].[NurseAvailability] (
    [nurse_availability_id] INT IDENTITY(1,1) NOT NULL,
    [staff_id] INT NOT NULL,
    [available_date] DATE NOT NULL,
    [is_available] BIT NOT NULL,
    [shift_type_id] INT NULL,
    [remarks] VARCHAR(255) NULL,
    CONSTRAINT [PK_NurseAvailability] PRIMARY KEY CLUSTERED ([nurse_availability_id] ASC)
) ON [PRIMARY];
GO

ALTER TABLE [dbo].[NurseAvailability] WITH CHECK ADD CONSTRAINT [FK_NurseAvailability_ShiftType] FOREIGN KEY([shift_type_id]) REFERENCES [dbo].[ShiftType]([shift_type_id]);
ALTER TABLE [dbo].[NurseAvailability] WITH CHECK ADD CONSTRAINT [FK_NurseAvailability_Staff] FOREIGN KEY([staff_id]) REFERENCES [dbo].[Staff]([staff_id]);
GO

-- =============================================
-- Table: LeaveStatus
-- =============================================
CREATE TABLE [dbo].[LeaveStatus] (
    [leave_status_id] INT IDENTITY(1,1) NOT NULL,
    [leave_status_name] NVARCHAR(20) NOT NULL UNIQUE,
    CONSTRAINT [PK_LeaveStatus] PRIMARY KEY ([leave_status_id])
);
GO

INSERT INTO [dbo].[LeaveStatus] (leave_status_name) VALUES ('Pending'), ('Approved'), ('Rejected');
GO

-- =============================================
-- Table: LeaveTypes
-- =============================================
CREATE TABLE [dbo].[LeaveTypes] (
    [leave_type_id] INT IDENTITY(1,1) NOT NULL,
    [leave_type_name] NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT [PK_LeaveTypes] PRIMARY KEY ([leave_type_id])
);
GO

INSERT INTO [dbo].[LeaveTypes] (leave_type_name) VALUES ('Sick'), ('Casual'), ('Vacation');
GO

-- =============================================
-- Table: LeaveRequests
-- =============================================
CREATE TABLE [dbo].[LeaveRequests] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [staff_id] INT NOT NULL,
    [leave_start] DATE NOT NULL,
    [leave_end] DATE NOT NULL,
    [leave_type_id] INT NOT NULL,
    [leave_status_id] INT NOT NULL,
    CONSTRAINT [PK_LeaveRequests] PRIMARY KEY ([id])
) ON [PRIMARY];
GO

ALTER TABLE [dbo].[LeaveRequests] WITH CHECK ADD CONSTRAINT [FK_LeaveRequests_Staff] FOREIGN KEY([staff_id]) REFERENCES [dbo].[Staff]([staff_id]);
ALTER TABLE [dbo].[LeaveRequests] WITH CHECK ADD CONSTRAINT [FK_LeaveRequests_LeaveTypes] FOREIGN KEY([leave_type_id]) REFERENCES [dbo].[LeaveTypes]([leave_type_id]);
ALTER TABLE [dbo].[LeaveRequests] WITH CHECK ADD CONSTRAINT [FK_LeaveRequests_LeaveStatus] FOREIGN KEY([leave_status_id]) REFERENCES [dbo].[LeaveStatus]([leave_status_id]);
GO
