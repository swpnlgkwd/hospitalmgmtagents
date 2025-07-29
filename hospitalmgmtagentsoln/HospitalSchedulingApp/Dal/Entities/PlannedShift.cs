using HospitalSchedulingApp.Common;

namespace HospitalSchedulingApp.Dal.Entities
{
    public class PlannedShift
    {
        public int PlannedShiftId { get; set; }
        public DateTime ShiftDate { get; set; }
        public ShiftTypes ShiftTypeId { get; set; }
        public int DepartmentId { get; set; }
        public int SlotNumber { get; set; }
        public ShiftStatuses ShiftStatusId { get; set; }
        public int? AssignedStaffId { get; set; }
    }
}
