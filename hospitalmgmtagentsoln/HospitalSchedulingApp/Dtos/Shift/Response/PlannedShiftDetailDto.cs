namespace HospitalSchedulingApp.Dtos.Shift.Response
{
    public class PlannedShiftDetailDto: PlannedShiftDto
    {

        // References (for logic/updating)
        public int ShiftTypeId { get; set; }
        public int DepartmentId { get; set; }
        public int ShiftStatusId { get; set; }
        public int? AssignedStaffId { get; set; }

        // Resolved values (for AI/UI)
        public string ShiftStatusName { get; set; } = string.Empty;
        public string AssignedStaffDepartmentName { get; set; } = string.Empty;
    }
}
