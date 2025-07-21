using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Model
{
    public class FindStaffResult
    {
        [JsonPropertyName("staffId")]
        public int StaffId { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        //[JsonPropertyName("role")]
        //public string? Role { get; set; }

        [JsonPropertyName("department")]
        public required string Department { get; set; }

        //[JsonPropertyName("specialty")]
        //public required string Specialty { get; set; }
    }
}
