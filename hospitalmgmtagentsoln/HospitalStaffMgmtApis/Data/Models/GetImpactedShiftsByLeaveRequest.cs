using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Model
{
    public class GetImpactedShiftsByLeaveRequest
    {
        [JsonPropertyName("staffId")]
        public int StaffId { get; set; }

        [JsonPropertyName("fromDate")]
        public DateTime FromDate { get; set; }

        [JsonPropertyName("toDate")]
        public DateTime ToDate { get; set; }
    }
}
