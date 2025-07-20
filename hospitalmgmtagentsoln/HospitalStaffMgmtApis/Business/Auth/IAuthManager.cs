using HospitalStaffMgmtApis.Data.Model.Account;

namespace HospitalStaffMgmtApis.Business.Auth
{
    public interface IAuthManager
    {
        Task<LoginResponse?> ValidateLoginAsync(LoginRequest request);
    }
}
