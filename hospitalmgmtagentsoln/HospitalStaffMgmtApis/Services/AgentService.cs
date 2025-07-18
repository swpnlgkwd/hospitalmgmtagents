using Azure;
using Azure.AI.Agents;
using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.Tools;
using HospitalStaffMgmtApis.Data.Models;
using HospitalStaffMgmtApis.Data.Repository;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents
{
    public class AgentService
    {
        private readonly PersistentAgentsClient _client;
        private readonly PersistentAgent _agent;
        private readonly IStaffRepository _staffRepository;
        private readonly ILogger<AgentService> _logger;

        public AgentService(PersistentAgentsClient persistentAgentsClient, PersistentAgent agent,
            IStaffRepository staffRepository, ILogger<AgentService> logger)
        {
            _client = persistentAgentsClient;
            _agent = agent;
            _staffRepository = staffRepository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new thread for a persistent agent conversation.
        /// </summary>
        public PersistentAgentThread CreateThread()
        {
            return _client.Threads.CreateThread();
        }

        /// <summary>
        /// Adds a user message to the thread.
        /// </summary>
        public Task AddUserMessageAsync(PersistentAgentThread thread, MessageRole role, string message)
        {
            _logger.LogInformation($"Adding user message to thread {thread.Id}: {message} :");
            _client.Messages.CreateMessage(thread.Id, role, message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Starts a run and retrieves the final response message from the agent.
        /// </summary>
        public async Task<MessageContent?> GetAgentResponseAsync(MessageRole role, string message)
        {
            // Step 1: Create a new thread and add user message
            var thread = CreateThread();
            await AddUserMessageAsync(thread, role, message);

            // Step 2: Start a new run
            ThreadRun run = _client.Runs.CreateRun(thread.Id, _agent.Id);

            // Step 3: Poll for completion
            do
            {
                Thread.Sleep(500); // Polling interval
                run = _client.Runs.GetRun(thread.Id, run.Id);

                // If tools are required, resolve and submit tool outputs
                if (run.Status == RunStatus.RequiresAction &&
                    run.RequiredAction is SubmitToolOutputsAction submitToolOutputsAction)
                {
                    var toolOutputs = new List<ToolOutput>();

                    foreach (var toolCall in submitToolOutputsAction.ToolCalls)
                    {
                        var result = await GetResolvedToolOutput(toolCall);
                        if (result != null)
                            toolOutputs.Add(result);
                    }
                    run = _client.Runs.SubmitToolOutputsToRun(thread.Id, run.Id, toolOutputs);
                }
            }
            while (run.Status == RunStatus.Queued ||
                   run.Status == RunStatus.InProgress ||
                   run.Status == RunStatus.RequiresAction);

            // Step 4: Return the last agent response
            var messages = _client.Messages.GetMessages(
                threadId: thread.Id,
                order: ListSortOrder.Descending
            );

            // Return the latest assistant message (if any)
            foreach (var msg in messages)
            {
                var messg = msg.ContentItems.OfType<MessageTextContent>().FirstOrDefault();
                _logger.LogInformation(messg?.Text.ToString());
                return messg;
            }
          
            return null;
        }

        /// <summary>
        /// Handles resolving specific tool calls triggered by the agent.
        /// </summary>
        private async Task<ToolOutput?> GetResolvedToolOutput(RequiredToolCall toolCall)
        {
            if (toolCall is RequiredFunctionToolCall functionToolCall)
            {
                using var argumentsJson = JsonDocument.Parse(functionToolCall.Arguments);

                Console.WriteLine($"Tool invoked: {toolCall.Id} at {DateTime.Now} for {functionToolCall.Name}");
                var root = argumentsJson.RootElement;
                _logger.LogInformation("Arguments Received : " + argumentsJson.ToString());
                _logger.LogInformation("Functions called: " + functionToolCall.Name);


                if (functionToolCall.Name == ShiftSwapTool.GetTool().Name)
                {
                    var request = new ShiftSwapRequest
                    {
                        ShiftDate = root.TryGetProperty("shiftDate", out var shiftDateProp) ? shiftDateProp.GetString() ?? "" : "",
                        ShiftType = root.TryGetProperty("shiftType", out var shiftTypeProp) ? shiftTypeProp.GetString() ?? "" : "",
                        OriginalStaffId = root.TryGetProperty("originalStaffId", out var origIdProp) ? origIdProp.GetInt32() : 0,
                        ReplacementStaffId = root.TryGetProperty("replacementStaffId", out var replIdProp) ? replIdProp.GetInt32() : 0
                    };
                    var resultMessage = await _staffRepository.ShiftSwapAsync(request);
                    var resultJson = JsonSerializer.Serialize(new { message = resultMessage });
                    _logger.LogInformation($"Data Retrieved for : {functionToolCall.Name} {resultJson} ");
                    return new ToolOutput(functionToolCall.Id, resultJson);
                }

                if (functionToolCall.Name == CancelShiftAssignmentTool.GetTool().Name)
                {
                    var cancelRequest = new CancelShiftRequest
                    {
                        StaffId = root.GetProperty("staffId").GetInt32(),
                        ShiftDate = root.GetProperty("shiftDate").GetString() ?? "",
                        ShiftType = root.GetProperty("shiftType").GetString() ?? ""
                    };

                    var result = await _staffRepository.CancelShiftAssignmentAsync(cancelRequest);
                    var resultJson = JsonSerializer.Serialize(new { message = result });
                    _logger.LogInformation($"Data Retrieved for : {functionToolCall.Name} {resultJson} ");
                    return new ToolOutput(functionToolCall.Id, JsonSerializer.Serialize(new { message = result }));
                }

                if (functionToolCall.Name == SubmitLeaveRequestTool.GetTool().Name)
                {
                    var leaveRequest = new LeaveRequest
                    {
                        StaffId = root.GetProperty("staffId").GetInt32(),
                        LeaveStart = root.GetProperty("leaveStart").GetString() ?? "",
                        LeaveEnd = root.GetProperty("leaveEnd").GetString() ?? "",
                        LeaveType = root.GetProperty("leaveType").GetString() ?? ""
                    };

                    var result = await _staffRepository.SubmitLeaveRequest(leaveRequest);
                    var resultJson = JsonSerializer.Serialize(new { message = result });
                    _logger.LogInformation($"Data Retrieved for : {functionToolCall.Name} {resultJson} ");
                    return new ToolOutput(functionToolCall.Id, JsonSerializer.Serialize(new { message = result }));
                }

                if (functionToolCall.Name == FetchStaffScheduleTool.GetTool().Name)
                {
                    var request = new StaffScheduleRequest
                    {
                        StaffId = root.GetProperty("staffId").GetInt32()
                    };

                    var schedule = await _staffRepository.FetchStaffSchedule(request);
                    var resultJson = JsonSerializer.Serialize(new { message = schedule });
                    _logger.LogInformation($"Data Retrieved for : {functionToolCall.Name} {resultJson} ");
                    return new ToolOutput(functionToolCall.Id, JsonSerializer.Serialize(schedule));
                }

                if (functionToolCall.Name == AssignShiftToStaffTool.GetTool().Name)
                {
                    var request = new AssignShiftRequest
                    {
                        StaffId = root.GetProperty("staffId").GetInt32(),
                        ShiftDate = root.GetProperty("shiftDate").GetString() ?? "",
                        ShiftType = root.GetProperty("shiftType").GetString() ?? ""
                    };

                    var result = await _staffRepository.AssignShiftToStaff(request);
                    var resultJson = JsonSerializer.Serialize(new { message = result });
                    _logger.LogInformation($"Data Retrieved for : {functionToolCall.Name} {resultJson} ");
                    return new ToolOutput(functionToolCall.Id, JsonSerializer.Serialize(new { message = result }));
                }

                if (functionToolCall.Name == FetchShiftCalendarTool.GetTool().Name)
                {
                    var request = new ShiftCalendarRequest
                    {
                        StartDate = root.GetProperty("startDate").GetString() ?? "",
                        EndDate = root.GetProperty("endDate").GetString() ?? ""
                    };

                    var result = await _staffRepository.FetchShiftCalendar(request);
                    var resultJson = JsonSerializer.Serialize(new { message = result });
                    _logger.LogInformation($"Data Retrieved for : {functionToolCall.Name} {resultJson} ");
                    return new ToolOutput(functionToolCall.Id, JsonSerializer.Serialize(result));
                }

                if (functionToolCall.Name == FindAvailableStaffTool.GetTool().Name)
                {

                    var request = new FindStaffRequest
                    {
                        ShiftDate = root.TryGetProperty("shiftDate", out var shiftDateProp) ? shiftDateProp.GetString() ?? "" : "",
                        ShiftType = root.TryGetProperty("shiftType", out var shiftTypeProp) ? shiftTypeProp.GetString() ?? "" : "",
                        Role = root.TryGetProperty("role", out var roleProp) ? roleProp.GetString() : null,
                        Department = root.TryGetProperty("department", out var deptProp) ? deptProp.GetString() : null
                    };

                    var staffResults = await _staffRepository.FindAvailableStaffAsync(request);
                    var resultJson = JsonSerializer.Serialize(staffResults, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    _logger.LogInformation($"Data Retrieved for : {functionToolCall.Name} {resultJson} ");
                    return new ToolOutput(functionToolCall.Id, resultJson);
                }
            }

            return null;
        }

    }
}
