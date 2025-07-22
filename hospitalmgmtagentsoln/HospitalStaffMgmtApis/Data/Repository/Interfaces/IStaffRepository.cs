using HospitalStaffMgmtApis.Data.Model;
using HospitalStaffMgmtApis.Data.Model.HospitalStaffMgmtApis.Models.Requests.HospitalStaffMgmtApis.Models.Requests;
using HospitalStaffMgmtApis.Data.Models;
using HospitalStaffMgmtApis.Models;
using HospitalStaffMgmtApis.Models.Requests;

namespace HospitalStaffMgmtApis.Data.Repository.Interfaces
{
    /// <summary>
    /// Interface defining the contract for all staff-related data operations including 
    /// scheduling, leave, shift replacement, and smart suggestions.
    /// </summary>
    public interface IStaffRepository
    {
        /// <summary>
        /// Resolves staff names matching the partial input provided.
        /// Used for auto-suggest/autocomplete functionality.
        /// </summary>
        /// <param name="namePart">Partial name string to search for.</param>
        /// <returns>List of staff members matching the input.</returns>
        Task<List<StaffIdResult>> ResolveStaffNameAsync(string namePart);

        /// <summary>
        /// Retrieves the shift schedule based on staff ID or department within a specific date range.
        /// </summary>
        /// <param name="shiftScheduleRequest">Filter criteria including staff/department and date range.</param>
        /// <returns>List of shifts matching the filter criteria.</returns>
        Task<List<ShiftScheduleResponse>> GetShiftScheduleAsync(ShiftScheduleRequest shiftScheduleRequest);

        /// <summary>
        /// Submits a leave request for a specific staff member.
        /// </summary>
        /// <param name="request">Leave request containing staff ID and date range.</param>
        /// <returns>True if the request was submitted successfully; otherwise false.</returns>
        Task<bool> ApplyForLeaveAsync(ApplyForLeaveRequest request);

        /// <summary>
        /// Automatically finds and assigns replacement staff for all shifts impacted by a leave request.
        /// </summary>
        /// <param name="request">Leave period to evaluate for shift coverage gaps.</param>
        /// <returns>Result including replaced shifts and selected staff.</returns>
        Task<AutoReplaceShiftsForLeaveResponse> AutoReplaceShiftsForLeaveAsync(GetImpactedShiftsByLeaveRequest request);

        /// <summary>
        /// Retrieves the shift schedule within a specific date range.
        /// </summary>
        /// <param name="startDate">Start date of the range.</param>
        /// <param name="endDate">End date of the range.</param>
        /// <returns>List of scheduled shifts within the range.</returns>
        Task<List<ShiftScheduleResponse>> GetShiftScheduleBetweenDatesAsync(DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Attempts to swap two shifts between staff members as per the request details.
        /// </summary>
        /// <param name="request">Shift swap request with involved staff and shifts.</param>
        /// <returns>True if the swap was successful; otherwise false.</returns>
        Task<bool> SwapShiftsAsync(SwapShiftRequest request);

        /// <summary>
        /// Fetches shifts for a given staff member within a defined date range.
        /// </summary>
        /// <param name="staffId">ID of the staff member.</param>
        /// <param name="startDate">Start date of the query range.</param>
        /// <param name="endDate">End date of the query range.</param>
        /// <returns>List of shifts assigned to the staff member in that time frame.</returns>
        Task<List<ShiftScheduleResponse>> FetchShiftInformationByStaffId(int staffId, DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Resolves a department's ID based on its name.
        /// </summary>
        /// <param name="departmentName">Name of the department.</param>
        /// <returns>Department ID if found; otherwise null.</returns>
        Task<int?> ResolveDepartmentIdAsync(string departmentName);

        /// <summary>
        /// Finds available staff members for a given shift type, department, and date.
        /// </summary>
        /// <param name="findAvailableStaffRequest">Search parameters for availability.</param>
        /// <returns>List of available and eligible staff for assignment.</returns>
        Task<List<FindAvailableStaffResponse>> FindAvailableStaffAsync(FindAvailableStaffRequest findAvailableStaffRequest);

        /// <summary>
        /// Retrieves all uncovered shifts (shifts that are not assigned to any staff).
        /// </summary>
        /// <returns>List of unassigned or uncovered planned shifts.</returns>
        Task<List<PlannedShift>> GetUncoveredShiftsAsync();

        /// <summary>
        /// Retrieves all leave requests that are still pending approval.
        /// </summary>
        /// <returns>List of pending leave requests for review.</returns>
        Task<List<LeaveRequest>> GetPendingLeaveRequestsAsync();

        /// <summary>
        /// Retrieves all leave requests that are still pending approval.
        /// </summary>
        /// <returns>List of pending leave requests for review.</returns>
        Task<List<PendingLeaveResponse>> FetchPendingLeaveRequestsAsync(PendingLeaveRequest pendingLeaveRequest);

        /// <summary>
        /// Retrieves all uncovered shifts (shifts that are not assigned to any staff).
        /// </summary>
        /// <returns>List of unassigned or uncovered planned shifts.</returns>
        Task<List<PlannedShift>> GetUncoveredShiftsAsync(GetUncoveredShiftsRequest request);


        /// <summary>
        /// Retrieves all fatique shifts (shifts that are not assigned to any staff).
        /// </summary>
        /// <returns>List of planned shifts.</returns>
        Task<List<PlannedShift>> GetFatiguedStaffAsync();
    }
}
