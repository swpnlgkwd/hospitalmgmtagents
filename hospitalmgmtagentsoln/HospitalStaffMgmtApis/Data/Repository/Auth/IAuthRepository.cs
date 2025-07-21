using HospitalStaffMgmtApis.Data.Model.Auth;

namespace HospitalStaffMgmtApis.Data.Repository.Auth
{
    public interface IAuthRepository
    {
        Task<LoginQueryResult?> GetLoginInfoByUsernameAsync(string username);
    }

}
