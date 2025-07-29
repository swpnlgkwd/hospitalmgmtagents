using HospitalSchedulingApp.Dal.Entities;
using HospitalSchedulingApp.Dtos.Shift.Requests;
using HospitalSchedulingApp.Dtos.Shift.Response;

namespace HospitalSchedulingApp.Services.Interfaces
{
    public interface IPlannedShiftService
    {
        Task<List<PlannedShiftDto>> FetchPlannedShiftsAsync(DateTime startDate, DateTime endDate);

        Task<List<PlannedShiftDetailDto>> FetchDetailedPlannedShiftsAsync(DateTime startDate, DateTime endDate);

        Task<List<PlannedShiftDetailDto>> FetchFilteredPlannedShiftsAsync(ShiftFilterDto filter);

    }
}
