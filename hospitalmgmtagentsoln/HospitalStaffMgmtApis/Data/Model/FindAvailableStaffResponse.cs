using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Model
{

    /// <summary>
    /// Request model for finding available staff for a potential or existing shift.
    /// </summary>
    public class FindAvailableStaffResponse
    {
        [JsonPropertyName("staffId")]
        public int StaffId { get; set; }

        [JsonPropertyName("staffName")]
        public string StaffName { get; set; } = string.Empty;

        /// <summary>
        /// Department name (e.g., ICU, Emergency)
        /// </summary>
        [JsonPropertyName("departmentName")]
        public string DepartmentName { get; set; } = string.Empty;

        /// <summary>
        /// Role name (e.g., Nurse, Doctor)
        /// </summary>
        [JsonPropertyName("roleName")]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Date on which the staff is available
        /// </summary>
        [JsonPropertyName("availableDate")]
        public DateTime? AvailableDate { get; set; }

        /// <summary>
        /// Shift name (e.g., Morning, Evening)
        /// </summary>
        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; } = string.Empty;
    }
}
