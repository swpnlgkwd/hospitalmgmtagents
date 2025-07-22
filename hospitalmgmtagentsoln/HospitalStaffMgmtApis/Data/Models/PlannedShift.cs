using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models
{
    public class PlannedShift
    {
        [JsonPropertyName("plannedShiftId")]
        public int PlannedShiftId { get; set; }

        [JsonPropertyName("shiftDate")]
        public DateTime ShiftDate { get; set; }

        [JsonPropertyName("shiftTypeName")]
        public string ShiftTypeName { get; set; } = string.Empty;

        [JsonPropertyName("departmentName")]
        public string DepartmentName { get; set; } = string.Empty;

        [JsonPropertyName("slotNumber")]
        public int SlotNumber { get; set; }

        [JsonPropertyName("shiftStatusId")]
        public int ShiftStatusId { get; set; }

        [JsonPropertyName("assignedStaffId")]
        public int? AssignedStaffId { get; set; }
    }
}
