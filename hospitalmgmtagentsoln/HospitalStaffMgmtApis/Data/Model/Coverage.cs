using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Model
{
    public class Coverage
    {

        [JsonPropertyName("fromDate")]
        public DateTime FromDate { get; set; }

        [JsonPropertyName("toDate")]
        public DateTime ToDate { get; set; }

        [JsonPropertyName("shiftType")]
        public string ShiftType { get; set; } = string.Empty;

        [JsonPropertyName("department")]
        public string Department { get; set; } = string.Empty;

    }


}
