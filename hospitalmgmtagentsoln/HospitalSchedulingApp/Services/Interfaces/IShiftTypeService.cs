using HospitalSchedulingApp.Dal.Entities;

namespace HospitalSchedulingApp.Services.Interfaces
{
    public interface IShiftTypeService
    {
        Task<ShiftType> FetchShiftInfoByShiftName(string shiftTypePart);
    }
}
