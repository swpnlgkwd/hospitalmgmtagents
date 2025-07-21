using HospitalStaffMgmtApis.Data.Model.Auth;

namespace HospitalStaffMgmtApis.Data.Repository.Interfaces
{
    public interface IAuthRepository
    {
        Task<LoginQueryResult?> GetLoginInfoByUsernameAsync(string username);
    }

}
