using HospitalSchedulingApp.Common;

namespace HospitalSchedulingApp.Dal.Entities
{

    public class NurseAvailability
    {
        public int NurseAvailabilityId { get; set; }
        public int StaffId { get; set; }
        public DateTime AvailableDate { get; set; }
        public bool IsAvailable { get; set; }
        public ShiftTypes? ShiftTypeId { get; set; }
        public string? Remarks { get; set; }
    }

}
