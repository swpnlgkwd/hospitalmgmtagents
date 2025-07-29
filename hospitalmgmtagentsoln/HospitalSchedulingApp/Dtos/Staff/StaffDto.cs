namespace HospitalSchedulingApp.Dtos.Staff
{
    public class StaffDto
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } =  string.Empty;
        public int StaffDepartmentId { get; set; }
        public string StaffDepartmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
