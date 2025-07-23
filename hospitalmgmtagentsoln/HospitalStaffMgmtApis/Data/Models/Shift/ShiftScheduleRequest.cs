using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models.Shift
{
    public class ShiftScheduleRequest
    {
        [JsonPropertyName("staffId")]
        public int? StaffId { get; set; } // Optional

        [JsonPropertyName("departmentId")]
        public int? DepartmentId { get; set; } // Optional

        [JsonPropertyName("fromDate")]
        public DateOnly? FromDate { get; set; }

        [JsonPropertyName("toDate")]
        public DateOnly? ToDate { get; set; }

        [JsonPropertyName("shiftType")]
        public string? ShiftType { get; set; }
    }

}
