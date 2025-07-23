namespace HospitalStaffMgmtApis.Data.Repository.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<int?> ResolveDepartmentIdAsync(string departmentName);

        Task<int?> GetDepartmentIdByNameAsync(string departmentName);
    }
}
