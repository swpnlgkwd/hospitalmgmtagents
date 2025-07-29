namespace HospitalSchedulingApp.Dal.Entities
{

    public class ShiftType
    {
        public int ShiftTypeId { get; set; }
        public string ShiftTypeName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
