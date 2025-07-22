using HospitalStaffMgmtApis.Business.Interfaces;
using HospitalStaffMgmtApis.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalStaffMgmtApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmartSuggestionsController : ControllerBase
    {
        private readonly ISchedulerSuggestionService _suggestionService;
        private readonly ILogger<SmartSuggestionsController> _logger;

        public SmartSuggestionsController(
            ISchedulerSuggestionService suggestionService,
            ILogger<SmartSuggestionsController> logger)
        {
            _suggestionService = suggestionService;
            _logger = logger;
        }

        /// <summary>
        /// Returns AI-generated smart scheduling suggestions for uncovered shifts.
        /// </summary>
        /// <returns>List of smart suggestions.</returns>
        [HttpGet]
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
    }
}
