using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Model
{
    public class AutoAssignShiftRequest
    {
        [JsonPropertyName("staffId")]
        public int StaffId { get; set; }

        [JsonPropertyName("shiftDate")]
        public string ShiftDate { get; set; } = string.Empty; // e.g., "2025-07-20"

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; } = string.Empty; // e.g., "Morning"

        [JsonPropertyName("shiftId")]
        public int ShiftId { get; set; }   // e.g., "Morning"
    }
}
