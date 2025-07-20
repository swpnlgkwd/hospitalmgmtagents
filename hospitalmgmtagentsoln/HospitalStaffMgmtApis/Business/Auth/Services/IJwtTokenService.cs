namespace HospitalStaffMgmtApis.Business.Auth.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(int staffId, string name, string roleName);
    }
}
