using HospitalStaffMgmtApis.Business.Auth.Services;
using HospitalStaffMgmtApis.Data.Model.Account;
using HospitalStaffMgmtApis.Data.Repository.Auth;

namespace HospitalStaffMgmtApis.Business.Auth
{
    public class AuthManager : IAuthManager
    {
        private readonly IAuthRepository _authRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthManager(IAuthRepository authRepository, IJwtTokenService jwtTokenService)
        {
            _authRepository = authRepository;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<LoginResponse?> ValidateLoginAsync(LoginRequest request)
        {
            // Get stored hash and user info
            var userRecord = await _authRepository.GetLoginInfoByUsernameAsync(request.Username);
            if (userRecord == null)
                return null;

            // Verify password using BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, userRecord.password_hash);
            if (!isPasswordValid)
                return null;

            // Prepare response with JWT token
            var response = new LoginResponse
            {
                StaffId = userRecord.staff_id,
                Name = userRecord.name,
                Role = userRecord.role_name,
                Token = _jwtTokenService.GenerateToken(userRecord.staff_id, userRecord.name, userRecord.role_name)
            };

            return response;
        }
    }

}
