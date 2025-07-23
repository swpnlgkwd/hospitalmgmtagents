using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models.Staff
{
    public class FatiqueStaffRequest
    {
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("staffName")]
        public string StaffName { get; set; } = string.Empty; // e.g. Cardiology, Neurology
    }
}
