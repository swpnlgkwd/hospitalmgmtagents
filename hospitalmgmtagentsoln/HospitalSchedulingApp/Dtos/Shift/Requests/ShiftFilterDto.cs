namespace HospitalSchedulingApp.Dtos.Shift.Requests
{
    public class ShiftFilterDto
    {
        public string? DepartmentName { get; set; }
        public string? StaffName { get; set; }
        public string? ShiftTypeName { get; set; }
        public string? ShiftStatusName { get; set; } // <-- new
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }


}
