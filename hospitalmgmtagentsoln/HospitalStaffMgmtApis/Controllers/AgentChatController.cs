using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents;
using Microsoft.AspNetCore.Mvc;

namespace HospitalStaffMgmtApis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    
    public class AgentChatController : ControllerBase
    {
        private readonly AgentService _agentService;

        public AgentChatController(AgentService agentService)
        {
            _agentService = agentService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskAgent([FromBody] UserMessageRequest request)
        {
            var response = await _agentService.GetAgentResponseAsync(MessageRole.User, request.Message);

            if (response is MessageTextContent textResponse)
            {
                return Ok(new { reply = textResponse.Text });
            }

            return BadRequest("No valid response from agent.");
        }
    }

    public class UserMessageRequest
    {
        public string Message { get; set; } = string.Empty;
    }

}
