namespace HospitalSchedulingApp.Dal.Entities
{
    public class ShiftPlanning
    {
        public DateTime ShiftDate { get; set; }
        public int ShiftTypeId { get; set; }
        public int DepartmentId { get; set; }
        public int RequiredNurses { get; set; }
    }
}
