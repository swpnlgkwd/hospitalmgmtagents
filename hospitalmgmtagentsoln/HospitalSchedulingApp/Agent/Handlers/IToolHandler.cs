using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalSchedulingApp.Agent.Handlers
{
    // Common tool handler interface
    public interface IToolHandler
    {
        string ToolName { get; }
        Task<ToolOutput?> HandleAsync(RequiredFunctionToolCall call, JsonElement root);
    }
}
