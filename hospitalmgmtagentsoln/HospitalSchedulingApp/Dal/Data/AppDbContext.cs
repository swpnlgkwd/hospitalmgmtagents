using HospitalSchedulingApp.Dal.Entities;
using Microsoft.EntityFrameworkCore;


namespace HospitalSchedulingApp.Dal.Data// Replace with your actual namespace
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSets
        public DbSet<AgentConversations> AgentConversations { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<ShiftStatus> ShiftStatus { get; set; }
        public DbSet<ShiftType> ShiftType { get; set; }
        public DbSet<ShiftPlanning> ShiftPlanning { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<UserCredential> UserCredential { get; set; }
        public DbSet<PlannedShift> PlannedShift { get; set; }
        public DbSet<NurseAvailability> NurseAvailability { get; set; }
        public DbSet<LeaveStatus> LeaveStatus { get; set; }
        public DbSet<LeaveTypes> LeaveTypes { get; set; }
        public DbSet<LeaveRequests> LeaveRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //// No navigation properties, so keep it minimal
            //base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AgentConversations>(entity =>
            {
                entity.ToTable("AgentConversations");
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.ThreadId).HasColumnName("thread_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Department");
                entity.HasKey(e => e.DepartmentId);
                entity.Property(e => e.DepartmentId).HasColumnName("department_id");
                entity.Property(e => e.DepartmentName).HasColumnName("department_name");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.RoleName).HasColumnName("role_name");
            });

            modelBuilder.Entity<ShiftStatus>(entity =>
            {
                entity.ToTable("ShiftStatus");
                entity.HasKey(e => e.ShiftStatusId);
                entity.Property(e => e.ShiftStatusId).HasColumnName("shift_status_id");
                entity.Property(e => e.ShiftStatusName).HasColumnName("shift_status_name");
            });

            modelBuilder.Entity<ShiftType>(entity =>
            {
                entity.ToTable("ShiftType");
                entity.HasKey(e => e.ShiftTypeId);
                entity.Property(e => e.ShiftTypeId).HasColumnName("shift_type_id");
                entity.Property(e => e.ShiftTypeName).HasColumnName("shift_type_name");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.EndTime).HasColumnName("end_time");
            });

            modelBuilder.Entity<ShiftPlanning>(entity =>
            {
                entity.ToTable("ShiftPlanning");
                entity.HasKey(e => new { e.ShiftDate, e.ShiftTypeId, e.DepartmentId });
                entity.Property(e => e.ShiftDate).HasColumnName("shift_date");
                entity.Property(e => e.ShiftTypeId).HasColumnName("shift_type_id");
                entity.Property(e => e.DepartmentId).HasColumnName("department_id");
                entity.Property(e => e.RequiredNurses).HasColumnName("required_nurses");
            });

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.ToTable("Staff");
                entity.HasKey(e => e.StaffId);
                entity.Property(e => e.StaffId).HasColumnName("staff_id");
                entity.Property(e => e.StaffName).HasColumnName("staff_name");
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.StaffDepartmentId).HasColumnName("staff_department_id");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
            });

            modelBuilder.Entity<UserCredential>(entity =>
            {
                entity.ToTable("UserCredential");
                entity.HasKey(e => e.CredentialId);
                entity.Property(e => e.CredentialId).HasColumnName("credential_id");
                entity.Property(e => e.StaffId).HasColumnName("staff_id");
                entity.Property(e => e.Username).HasColumnName("username");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            });

            modelBuilder.Entity<PlannedShift>(entity =>
            {
                entity.ToTable("PlannedShift");
                entity.HasKey(e => e.PlannedShiftId);
                entity.Property(e => e.PlannedShiftId).HasColumnName("planned_shift_id");
                entity.Property(e => e.ShiftDate).HasColumnName("shift_date");
                entity.Property(e => e.ShiftTypeId).HasColumnName("shift_type_id");
                entity.Property(e => e.DepartmentId).HasColumnName("department_id");
                entity.Property(e => e.SlotNumber).HasColumnName("slot_number");
                entity.Property(e => e.ShiftStatusId).HasColumnName("shift_status_id");
                entity.Property(e => e.AssignedStaffId).HasColumnName("assigned_staff_id");
            });

            modelBuilder.Entity<NurseAvailability>(entity =>
            {
                entity.ToTable("NurseAvailability");
                entity.HasKey(e => e.NurseAvailabilityId);
                entity.Property(e => e.NurseAvailabilityId).HasColumnName("nurse_availability_id");
                entity.Property(e => e.StaffId).HasColumnName("staff_id");
                entity.Property(e => e.AvailableDate).HasColumnName("available_date");
                entity.Property(e => e.IsAvailable).HasColumnName("is_available");
                entity.Property(e => e.ShiftTypeId).HasColumnName("shift_type_id");
                entity.Property(e => e.Remarks).HasColumnName("remarks");
            });

            modelBuilder.Entity<LeaveStatus>(entity =>
            {
                entity.ToTable("LeaveStatus");
                entity.HasKey(e => e.LeaveStatusId);
                entity.Property(e => e.LeaveStatusId).HasColumnName("leave_status_id");
                entity.Property(e => e.LeaveStatusName).HasColumnName("leave_status_name");
            });

            modelBuilder.Entity<LeaveTypes>(entity =>
            {
                entity.ToTable("LeaveTypes");
                entity.HasKey(e => e.LeaveTypeId);
                entity.Property(e => e.LeaveTypeId).HasColumnName("leave_type_id");
                entity.Property(e => e.LeaveTypeName).HasColumnName("leave_type_name");
            });

            modelBuilder.Entity<LeaveRequests>(entity =>
            {
                entity.ToTable("LeaveRequests");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.StaffId).HasColumnName("staff_id");
                entity.Property(e => e.LeaveStart).HasColumnName("leave_start");
                entity.Property(e => e.LeaveEnd).HasColumnName("leave_end");
                entity.Property(e => e.LeaveTypeId).HasColumnName("leave_type_id");
                entity.Property(e => e.LeaveStatusId).HasColumnName("leave_status_id");
            });
        }
    }
}

