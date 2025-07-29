namespace HospitalSchedulingApp.Dal.Entities
{
    public class Staff
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int StaffDepartmentId { get; set; }
        public bool IsActive { get; set; }
    }
}
