using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents;
using HospitalStaffMgmtApis.Agents.Services;
using HospitalStaffMgmtApis.Data.Models.UserMessage;
using HospitalStaffMgmtApis.Data.Repository;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalStaffMgmtApis.Controllers
{
    /// <summary>
    /// API controller to handle chat-based interaction with the persistent AI agent.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]

    public class AgentChatController : ControllerBase
    {
        private readonly AgentService _agentService;
        private readonly IAgentConversationRepository _agentConversationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="agentService">Service to handle agent communication.</param>
        public AgentChatController(AgentService agentService, IAgentConversationRepository agentConversationRepository)
        {
            _agentService = agentService;
            _agentConversationRepository = agentConversationRepository;
        }

        /// <summary>
        /// Sends a user message to the persistent agent and returns the agent's response.
        /// </summary>
        /// <param name="request">The message input from the user.</param>
        /// <returns>A response from the agent or a bad request if the response is invalid.</returns>
        [HttpPost("ask")]
        public async Task<IActionResult> AskAgent([FromBody] UserMessageRequest request)
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
