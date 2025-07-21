using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.FunctionTools;
using HospitalStaffMgmtApis.Agents.Tools;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.Handlers
{
    /// <summary>
    /// Handler for the DepartmentNameResolver tool.
    /// Resolves a department name (partial or full) to its department ID.
    /// </summary>
    public class DepartmentNameResolverToolHandler : IToolHandler
    {
        private readonly IStaffRepository _repository;
        private readonly ILogger<DepartmentNameResolverToolHandler> _logger;

        public DepartmentNameResolverToolHandler(
            IStaffRepository repository,
            ILogger<DepartmentNameResolverToolHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public string ToolName => DepartmentNameResolverTool.GetTool().Name;

        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            string inputName = root.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString()?.Trim() ?? string.Empty
                : string.Empty;

            if (string.IsNullOrWhiteSpace(inputName))
            {
                _logger.LogWarning("DepartmentNameResolverTool: Department name was not provided.");
                return new ToolOutput(call.Id, JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "Department name is required."
                }));
            }

            if (inputName.Length < 2)
            {
                _logger.LogWarning("DepartmentNameResolverTool: Name '{Input}' is too short.", inputName);
                return new ToolOutput(call.Id, JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "Department name must be at least 2 characters long."
                }));
            }

            var departmentId = await _repository.ResolveDepartmentIdAsync(inputName);

            if (departmentId == null)
            {
                _logger.LogInformation("DepartmentNameResolverTool: No match found for '{Name}'", inputName);
                return new ToolOutput(call.Id, JsonSerializer.Serialize(new
                {
                    success = false,
                    error = $"No department found matching name: {inputName}"
                }));
            }

            var resultJson = JsonSerializer.Serialize(new
            {
                success = true,
                departmentId = departmentId.Value
            });

            _logger.LogInformation("DepartmentNameResolverTool: Resolved '{InputName}' to department ID {DepartmentId}",
                inputName, departmentId);

            return new ToolOutput(call.Id, resultJson);
        }
    }
}
