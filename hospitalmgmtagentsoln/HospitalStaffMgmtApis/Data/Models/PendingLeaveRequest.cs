using System;

namespace HospitalStaffMgmtApis.Models
{
    /// <summary>
    /// Represents the filter criteria for retrieving pending leave requests.
    /// Used by the ViewPendingLeaveRequestsTool.
    /// </summary>
    public class PendingLeaveRequest
    {
        /// <summary>
        /// Optional. Start date to filter leave requests (format: YYYY-MM-DD).
        /// Supports natural language phrases like "today" or "this week".
        /// </summary>
        public string? FromDate { get; set; }

        /// <summary>
        /// Optional. End date to filter leave requests (format: YYYY-MM-DD).
        /// Used in conjunction with FromDate to form a date range.
        /// </summary>
        public string? ToDate { get; set; }

        /// <summary>
        /// Optional. Full or partial name of the staff member whose pending leave requests should be retrieved.
        /// </summary>
        public string? StaffName { get; set; }

        /// <summary>
        /// Optional. Department name to filter leave requests (e.g., ICU, Pediatrics).
        /// </summary>
        public string? DepartmentName { get; set; }
    }
}
