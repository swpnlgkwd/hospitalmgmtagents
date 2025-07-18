using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models
{
    public class AssignShiftRequest
    {
        [JsonPropertyName("staffId")]
        public int StaffId { get; set; }

        [JsonPropertyName("shiftDate")]
        public string ShiftDate { get; set; } = string.Empty; // e.g., "2025-07-20"

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; } = string.Empty; // e.g., "Morning"
    }
}
