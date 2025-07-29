namespace HospitalSchedulingApp.Dtos.Shift.Response
{
    public class PlannedShiftDto
    {
        public int PlannedShiftId { get; set; }
        public DateTime ShiftDate { get; set; }
        public int SlotNumber { get; set; }
        public string ShiftTypeName { get; set; } = string.Empty;
        public string AssignedStaffFullName { get; set; } = string.Empty;
        public string ShiftDeparmentName { get; set; } = string.Empty;
    }

}
