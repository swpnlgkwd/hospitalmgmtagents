using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models.StaffLeaveRequest
{
    /// <summary>
    /// Represents a leave request submitted by a staff member.
    /// </summary>
    public class LeaveRequestResponse
    {
        [JsonPropertyName("leaveRequestId")]
        public int LeaveRequestId { get; set; }

        [JsonPropertyName("departmentName")]
        public string DepartmentName { get; set; } =string.Empty; // e.g. Cardiology, Neurology

        [JsonPropertyName("staffName")]
        public string StaffName { get; set; } =string.Empty; // e.g. John Doe

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty; // e.g. John Doe

        [JsonPropertyName("leaveStart")]
        public DateTime LeaveStartDate { get; set; }

        [JsonPropertyName("leaveEnd")]
        public DateTime LeaveEndDate { get; set; }

        [JsonPropertyName("status")]
        public string LeaveStatus { get; set; } = string.Empty; // e.g. Pending, Approved, Rejected
    }
}
