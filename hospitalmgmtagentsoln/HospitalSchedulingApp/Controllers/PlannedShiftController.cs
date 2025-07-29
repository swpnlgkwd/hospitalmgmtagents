using HospitalSchedulingApp.Services.Interfaces;
using HospitalSchedulingApp.Dal.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HospitalSchedulingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlannedShiftController : ControllerBase
    {
        private readonly ILogger<PlannedShiftController> _logger;
        private readonly IPlannedShiftService _plannedShiftService;

        public PlannedShiftController(
            ILogger<PlannedShiftController> logger,
            IPlannedShiftService plannedShiftService)
        {
            _logger = logger;
            _plannedShiftService = plannedShiftService;
        }

        [HttpGet("fetch")]
        public async Task<IActionResult> FetchPlannedShiftsBetweenDates(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest("Start date must be before end date.");
            }

            var result = await _plannedShiftService.FetchPlannedShiftsAsync(startDate, endDate);
            return Ok(result);
        }
    }
}
