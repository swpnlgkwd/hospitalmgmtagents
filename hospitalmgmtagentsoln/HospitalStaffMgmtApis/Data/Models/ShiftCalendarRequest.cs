using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models
{
    public class ShiftCalendarRequest
    {
        [JsonPropertyName("startDate")]
        public string StartDate { get; set; } = string.Empty;

        [JsonPropertyName("endDate")]
        public string EndDate { get; set; } = string.Empty;
    }
}
