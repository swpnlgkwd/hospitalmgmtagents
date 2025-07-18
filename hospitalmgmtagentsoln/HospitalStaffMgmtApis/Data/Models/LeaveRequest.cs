using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models
{
    public class LeaveRequest
    {
        [JsonPropertyName("staffId")]
        public int StaffId { get; set; }

        [JsonPropertyName("leaveStart")]
        public string LeaveStart { get; set; } = string.Empty; // e.g., "2025-07-20"

        [JsonPropertyName("leaveEnd")]
        public string LeaveEnd { get; set; } = string.Empty; // e.g., "2025-07-20"

        [JsonPropertyName("leaveType")]
        public string LeaveType { get; set; } = string.Empty; // e.g., "2025-07-20"

    }
}
