using HospitalStaffMgmtApis.Agents.Services;
using HospitalStaffMgmtApis.Business.Interfaces;
using HospitalStaffMgmtApis.Data.Models.SmartSuggestions;
using Microsoft.AspNetCore.Mvc;

namespace HospitalStaffMgmtApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmartSuggestionsController : ControllerBase
    {
        private readonly ISmartSuggestionManager _suggestionService;
        private readonly ILogger<SmartSuggestionsController> _logger;

        public SmartSuggestionsController(
            ISmartSuggestionManager suggestionService,
            ILogger<SmartSuggestionsController> logger)
        {
            _suggestionService = suggestionService;
            _logger = logger;
        }

        /// <summary>
        /// Returns AI-generated smart scheduling suggestions for uncovered shifts.
        /// </summary>
        /// <returns>List of smart suggestions.</returns>
        [HttpGet("suggestions")]
        public async Task<ActionResult<List<SmartSuggestion>>> GetSmartSuggestions()
        {
            try
            {
                var suggestions = await _suggestionService.GetSmartSuggestionsAsync();

                if (suggestions == null || suggestions.Count == 0)
                    return NoContent();

                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving smart shift suggestions.");
                return StatusCode(500, "Internal server error.");
            }
        }


        [HttpGet("agentinsights")]
        public async Task<ActionResult<string>> GetAgentInsights()
        {
            try
            {
                var suggestions = await _suggestionService.GetAgentInsights();

         
                return Ok(new
                {
                    message = suggestions
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving smart shift suggestions.");
                return StatusCode(500, "Internal server error.");
            }
        }

        // Backend controller
        [HttpGet("agent-scheduler")]
        public async Task<IActionResult> GetSchedulerGreeting()
        {
            var summary = await _suggestionService.GetDailySchedulerSummaryAsync();
            return Ok(summary);
        }

    }
}
