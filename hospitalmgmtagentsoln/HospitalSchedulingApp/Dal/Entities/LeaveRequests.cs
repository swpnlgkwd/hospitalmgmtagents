using HospitalSchedulingApp.Common;

namespace HospitalSchedulingApp.Dal.Entities
{
    public class LeaveRequests
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public DateTime LeaveStart { get; set; }
        public DateTime LeaveEnd { get; set; }
        public LeaveType LeaveTypeId { get; set; } 
        public LeaveRequestStatuses LeaveStatusId { get; set; }
    }
}
