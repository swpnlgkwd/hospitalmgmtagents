using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Model
{
    public class AutoReplaceShiftsForLeaveResult
    {
        [JsonPropertyName("shiftDate")]
        public DateTime ShiftDate { get; set; }

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; } = string.Empty;

        [JsonPropertyName("department")]
        public string Department { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("assignedTo")]
        public string? AssignedTo { get; set; } // Nullable in case not assigned

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}
