using HospitalStaffMgmtApis.Data.Model;
using HospitalStaffMgmtApis.Data.Models;
using HospitalStaffMgmtApis.Data.Models.StaffLeaveRequest;
using System.Threading.Tasks;

namespace HospitalStaffMgmtApis.Data.Repository.Interfaces
{
    public interface ILeaveRequestRepository
    {

        Task<List<PendingLeaveResponse>> FetchPendingLeaveRequestsAsync(PendingLeaveRequest? pendingLeaveRequest = null);

        Task<bool> ApplyForLeaveAsync(ApplyForLeaveRequest leaveRequest);

        Task<AutoReplaceShiftsForLeaveResponse> AutoReplaceShiftsForLeaveAsync(GetImpactedShiftsByLeaveRequest request);

        Task<List<LeaveImpactedShiftResponse>> FetchLeaveImpactedShiftsAsync(GetImpactedShiftsByLeaveRequest request);

        //Task<List<PendingLeaveResponse>> GetPendingLeaveRequestsAsync();
        Task<List<LeaveImpactedShiftResponse>?> ApproveOrRejectLeaveRequestAsync(LeaveActionRequest request);



    }
}
