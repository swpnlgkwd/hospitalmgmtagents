using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models
{
    /// <summary>
    /// Represents a leave request submitted by a staff member.
    /// </summary>
    public class LeaveRequest
    {
        [JsonPropertyName("leaveRequestId")]
        public int LeaveRequestId { get; set; }

        [JsonPropertyName("staffId")]
        public int StaffId { get; set; }

        [JsonPropertyName("leaveStart")]
        public DateTime LeaveStart { get; set; }

        [JsonPropertyName("leaveEnd")]
        public DateTime LeaveEnd { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty; // e.g. Pending, Approved, Rejected
    }
}
