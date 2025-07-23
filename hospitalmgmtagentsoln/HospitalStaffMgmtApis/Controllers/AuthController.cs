using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.Services;
using HospitalStaffMgmtApis.Business.Interfaces;
using HospitalStaffMgmtApis.Data.Model.Account;
using HospitalStaffMgmtApis.Data.Models.Agent;
using HospitalStaffMgmtApis.Data.Repository;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalStaffMgmtApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthManager _authManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly AgentService _agentService;
        private readonly IAgentConversationRepository _agentConversationRepository;
        public AuthorizationController(IAuthManager authManager, IJwtTokenService jwtTokenService,
            AgentService agentService,
            IAgentConversationRepository agentConversationRepository)
        {
            _authManager = authManager;
            _jwtTokenService = jwtTokenService;
            _agentService = agentService;
            _agentConversationRepository = agentConversationRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var response = await _authManager.ValidateLoginAsync(request);

            var thread = _agentService.CreateThread();

            var agentConversation = new AgentConversation
            {
                UserId = response?.StaffId.ToString() ?? "",
                ThreadId = thread.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _agentConversationRepository.AddThreadForUser(agentConversation);

            if (response == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(new { loginResponse = response, threadId = thread.Id });
            
        }


        [HttpPost("logout/{threadId}")]
        public async Task<IActionResult> Logout([FromQuery] string threadId)
        {
            await _agentConversationRepository.DeleteThreadForUser(threadId);
            await _agentService.DeleteThreadForUser(threadId);
            return Ok();
        }
    }
}
