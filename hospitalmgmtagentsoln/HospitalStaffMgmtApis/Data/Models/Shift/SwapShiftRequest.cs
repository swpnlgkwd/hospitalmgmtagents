using System;
using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Models.Shift
{

    public class SwapShiftRequest
    {
        [JsonPropertyName("staffId1")]
        public int StaffId1 { get; set; }

        [JsonPropertyName("shiftDate1")]
        public DateTime ShiftDate1 { get; set; }

        [JsonPropertyName("shiftTypeId1")]
        public int ShiftTypeId1 { get; set; }

        [JsonPropertyName("staffId2")]
        public int StaffId2 { get; set; }

        [JsonPropertyName("shiftDate2")]
        public DateTime ShiftDate2 { get; set; }

        [JsonPropertyName("shiftTypeId2")]
        public int ShiftTypeId2 { get; set; }
    }


}
