using HospitalSchedulingApp.Dal.Entities;

namespace HospitalSchedulingApp.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<Department?> FetchDepartmentInformationAsync(string departmentNamePart);
    }
}
