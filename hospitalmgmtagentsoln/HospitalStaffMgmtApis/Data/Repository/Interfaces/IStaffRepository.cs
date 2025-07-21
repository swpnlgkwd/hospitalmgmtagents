using HospitalStaffMgmtApis.Data.Model;
using HospitalStaffMgmtApis.Data.Model.HospitalStaffMgmtApis.Models.Requests.HospitalStaffMgmtApis.Models.Requests;

namespace HospitalStaffMgmtApis.Data.Repository.Interfaces
{
    // Interface defining the contract for staff-related data operations
    public interface IStaffRepository
    {
        // Resolve staff names based on partial input (for auto-suggest/autocomplete)
        Task<List<StaffIdResult>> ResolveStaffNameAsync(string namePart);

        // Fetch shift schedule for a specific staff member or department
        Task<List<ShiftScheduleResponse>> GetShiftScheduleAsync(ShiftScheduleRequest shiftScheduleRequest);

        // Submit leave request for a staff member
        Task<bool> ApplyForLeaveAsync(ApplyForLeaveRequest request);

        // Auto-replace shifts impacted due to leave by assigning alternative staff
        Task<AutoReplaceShiftsForLeaveResponse> AutoReplaceShiftsForLeaveAsync(GetImpactedShiftsByLeaveRequest request);

        Task<List<ShiftScheduleResponse>> GetShiftScheduleBetweenDatesAsync(DateOnly startDate, DateOnly endDate);
        Task<bool> SwapShiftsAsync(SwapShiftRequest request);
        Task<List<ShiftScheduleResponse>> FetchShiftInformationByStaffId(int staffId, DateOnly startDate, DateOnly endDate);


        // Resolve staff names based on partial input (for auto-suggest/autocomplete)
        Task<int?> ResolveDepartmentIdAsync(string departmentName);


        // find available staff 
        Task<List<FindAvailableStaffResponse>> FindAvailableStaffAsync(FindAvailableStaffRequest findAvailableStaffRequest);


        // Task FetchCoverageByDateRangeAsync(Coverage coverage);

    }

}
