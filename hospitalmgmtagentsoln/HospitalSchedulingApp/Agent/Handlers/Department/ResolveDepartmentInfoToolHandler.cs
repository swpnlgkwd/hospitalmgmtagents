
using Azure.AI.Agents.Persistent;
using HospitalSchedulingApp.Agent.Tools;
using HospitalSchedulingApp.Agent.Tools.Department;
using HospitalSchedulingApp.Dal.Repositories;
using HospitalSchedulingApp.Services.Interfaces;
using System.Text.Json;

namespace HospitalSchedulingApp.Agent.Handlers.Department
{ 

    public class ResolveDepartmentInfoToolHandler : IToolHandler
    {
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<ResolveDepartmentInfoToolHandler> _logger;

        public ResolveDepartmentInfoToolHandler(
            IDepartmentService departmentService,
            ILogger<ResolveDepartmentInfoToolHandler> logger)
        {
            _departmentService = departmentService;
            _logger = logger;
        }

        public string ToolName => ResolveDepartmentInfoTool.GetTool().Name;

        public async Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root)
        {
            string inputName = root.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString()?.Trim() ?? string.Empty
                : string.Empty;

            if (string.IsNullOrWhiteSpace(inputName))
            {
                _logger.LogWarning("resolveDepartmentInfo: No input name provided.");
                return CreateError(call.Id, "Department name is required.");
            }

            if (inputName.Length < 2)
            {
                _logger.LogWarning("resolveDepartmentInfo: Input '{Input}' is too short.", inputName);
                return CreateError(call.Id, "Department name must be at least 2 characters long.");
            }

            var department = await _departmentService.FetchDepartmentInformationAsync(inputName);

            if (department == null)
            {
                _logger.LogInformation("resolveDepartmentInfo: No department found for name '{Name}'", inputName);
                return CreateError(call.Id, $"No department found matching: {inputName}");
            }

            var result = new
            {
                success = true,
                department = new
                {
                    department.DepartmentId,
                    department.DepartmentName
                }
            };

            _logger.LogInformation("resolveDepartmentInfo: Matched '{Input}' to Department ID {Id} - {Name}",
                inputName, department.DepartmentId, department.DepartmentName);

            return new ToolOutput(call.Id, JsonSerializer.Serialize(result));
        }

        private ToolOutput CreateError(string callId, string message)
        {
            var errorJson = JsonSerializer.Serialize(new
            {
                success = false,
                error = message
            });

            return new ToolOutput(callId, errorJson);
        }
    }

}
