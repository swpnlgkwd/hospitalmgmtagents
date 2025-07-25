using System;
using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models.StaffLeaveRequest
{
    /// <summary>
    /// Represents the filter criteria for retrieving pending leave requests.
    /// Used by the ViewPendingLeaveRequestsTool.
    /// </summary>
    public class PendingLeaveResponse
    {
        [JsonPropertyName("leaveRequestId")]
        public int LeaveRequestId { get; set; }

        [JsonPropertyName("departmentName")]
        public string DepartmentName { get; set; } = string.Empty; // e.g. Cardiology, Neurology

        [JsonPropertyName("staffName")]
        public string StaffName { get; set; } = string.Empty; // e.g. John Doe

        [JsonPropertyName("staff_id")]
        public int StaffId { get; set; }  // e.g. 1234

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty; // e.g. Nurse, Doctor

        [JsonPropertyName("leaveStart")]
        public DateTime LeaveStartDate { get; set; }

        [JsonPropertyName("leaveEnd")]
        public DateTime LeaveEndDate { get; set; }

        [JsonPropertyName("leaveType")]
        public string LeaveType { get; set; } = string.Empty; // e.g. Sick Leave

        [JsonPropertyName("status")]
        public string LeaveStatus { get; set; } = string.Empty; // e.g. Pending
    }

}
