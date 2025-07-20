
using HospitalStaffMgmtApis.Business.Auth;
using HospitalStaffMgmtApis.Business.Auth.Services;
using HospitalStaffMgmtApis.Data.Model.Account;
using Microsoft.AspNetCore.Mvc;

namespace HospitalStaffMgmtApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthManager _authManager;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthorizationController(IAuthManager authManager, IJwtTokenService jwtTokenService)
        {
            _authManager = authManager;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var response = await _authManager.ValidateLoginAsync(request);
            if (response == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(response);
        }

    }
}
