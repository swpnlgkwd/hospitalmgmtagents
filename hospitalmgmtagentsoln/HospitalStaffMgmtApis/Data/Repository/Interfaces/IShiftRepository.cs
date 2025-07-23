using HospitalStaffMgmtApis.Data.Models;
using HospitalStaffMgmtApis.Data.Models.Shift;
using HospitalStaffMgmtApis.Models.Requests;

namespace HospitalStaffMgmtApis.Data.Repository.Interfaces
{
    public interface IShiftRepository
    {
        /// <summary>
        /// Retrieves the shift schedule based on staff ID or department within a specific date range.
        /// </summary>
        /// <param name="shiftScheduleRequest">Filter criteria including staff/department and date range.</param>
        /// <returns>List of shifts matching the filter criteria.</returns>
        Task<List<ShiftScheduleResponse>> GetShiftScheduleAsync(ShiftScheduleRequest shiftScheduleRequest);

        Task<List<ShiftScheduleResponse>> GetShiftScheduleBetweenDatesAsync(DateOnly startDate, DateOnly endDate);

        Task<bool> SwapShiftsAsync(SwapShiftRequest request);

        //Task<List<PlannedShift>> GetUncoveredShiftsAsync();

        Task<List<PlannedShift>> GetUncoveredShiftsAsync(GetUncoveredShiftsRequest? request = null);

        Task<List<ShiftScheduleResponse>> FetchShiftInformationByStaffId(int staffId, DateOnly startDate, DateOnly endDate);

        Task<int?> GetShiftTypeIdByNameAsync(string? shiftTypeName);
    }
}
