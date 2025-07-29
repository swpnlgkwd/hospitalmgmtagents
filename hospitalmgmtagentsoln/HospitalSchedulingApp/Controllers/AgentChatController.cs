using Azure.AI.Agents.Persistent;
using HospitalSchedulingApp.Agent.Services;
using HospitalSchedulingApp.Dal.Entities;
using HospitalSchedulingApp.Dtos.Agent;
using HospitalSchedulingApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalSchedulingApp.Controllers
{
    /// <summary>
    /// API controller to handle chat-based interaction with the persistent AI agent.
    /// </summary>
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class AgentChatController : ControllerBase
    {
        private readonly IAgentService _agentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="agentService">Service to handle agent communication.</param>
        public AgentChatController(IAgentService agentService) 
        {
            _agentService = agentService;
        }

        /// <summary>
        /// Sends a user message to the persistent agent and returns the agent's response.
        /// </summary>
        /// <param name = "request" > The message input from the user.</param>
        /// <returns>A response from the agent or a bad request if the response is invalid.</returns>
        [HttpPost("ask")]
        public async Task<IActionResult> AskAgent([FromBody] UserMessageRequestDto request)
        {
            var response = await _agentService.GetAgentResponseAsync(request.ThreadId, MessageRole.User, request.Message);

            if (response is MessageTextContent textResponse)
            {
                return Ok(new { reply = textResponse.Text });
            }

            return BadRequest("No valid response from agent.");
        }
    }
}
