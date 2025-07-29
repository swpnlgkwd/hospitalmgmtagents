using Azure.Core;
using HospitalSchedulingApp.Dtos.Auth;
using HospitalSchedulingApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalSchedulingApp.Controllers
{
    /// <summary>
    /// API controller to handle login operations for staff users.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Authenticates a user based on username and password.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var response = await _authService.Login(request);

            if (response == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(new { loginResponse = response });
        }

       
       [HttpPost("logout/{threadId}")]
        public async Task<IActionResult> Logout(string threadId)
        {
            await _authService.Logout(threadId);
            return Ok();
        }
    }
}
