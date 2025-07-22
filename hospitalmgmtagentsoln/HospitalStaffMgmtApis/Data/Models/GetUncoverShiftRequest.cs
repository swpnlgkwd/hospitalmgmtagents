using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Models.Requests
{
    /// <summary>
    /// Represents optional filters for querying uncovered (unassigned) planned shifts.
    /// </summary>
    public class GetUncoveredShiftsRequest
    {
        /// <summary>
        /// Start date of the date range (inclusive). Defaults to today if not provided.
        /// </summary>
        [JsonPropertyName("fromDate")]
        public DateTime FromDate { get; set; } = DateTime.Today;

        /// <summary>
        /// Optional end date of the date range (inclusive).
        /// </summary>
        [JsonPropertyName("toDate")]
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Optional department ID to filter uncovered shifts by department.
        /// </summary>
        [JsonPropertyName("departmentId")]
        public int? DepartmentId { get; set; }

        /// <summary>
        /// Optional role of the staff typically required for the shift (e.g., "Nurse", "Doctor").
        /// </summary>
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        /// <summary>
        /// Optional shift type name to filter (e.g., "Morning", "Night").
        /// </summary>
        [JsonPropertyName("shiftType")]
        public string? ShiftType { get; set; }
    }
}
