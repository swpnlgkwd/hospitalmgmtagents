using Azure.AI.Agents.Persistent;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using HospitalStaffMgmtApis.Agents.Tools;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;

namespace HospitalStaffMgmtApis.Agents.Handlers
{
    /// <summary>
    /// Handler for the StaffNameResolver tool.
    /// Resolves partial or full staff names and returns matching staff records.
    /// </summary>
    public class StaffNameResolverToolHandler : IToolHandler
    {
        private readonly IStaffRepository _repository;
        private readonly ILogger<StaffNameResolverToolHandler> _logger;

        public StaffNameResolverToolHandler(
            IStaffRepository repository,
            ILogger<StaffNameResolverToolHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public string ToolName => StaffNameResolverTool.GetTool().Name;

        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            string inputName = root.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString()?.Trim() ?? string.Empty
                : string.Empty;

            if (string.IsNullOrWhiteSpace(inputName))
            {
                _logger.LogWarning("StaffNameResolverTool: Name was not provided.");
                return new ToolOutput(call.Id, JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "Staff name is required."
                }));
            }

            if (inputName.Length < 2)
            {
                _logger.LogWarning("StaffNameResolverTool: Name '{Input}' is too short.", inputName);
                return new ToolOutput(call.Id, JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "Staff name must be at least 2 characters long."
                }));
            }

            var matchingStaff = await _repository.ResolveStaffNameAsync(inputName);

            if (matchingStaff == null || matchingStaff.Count == 0)
            {
                _logger.LogInformation("StaffNameResolverTool: No match found for '{Name}'", inputName);
                return new ToolOutput(call.Id, JsonSerializer.Serialize(new
                {
                    success = false,
                    error = $"No active staff found matching name: {inputName}"
                }));
            }

            var resultJson = JsonSerializer.Serialize(new
            {
                success = true,
                matches = matchingStaff.Select(s => new
                {
                    staffId = s.StaffId,
                    name = s.Name
                })
            });

            _logger.LogInformation("StaffNameResolverTool: Resolved {Count} matches for name '{InputName}'", matchingStaff.Count, inputName);
            return new ToolOutput(call.Id, resultJson);
        }
    }
}
