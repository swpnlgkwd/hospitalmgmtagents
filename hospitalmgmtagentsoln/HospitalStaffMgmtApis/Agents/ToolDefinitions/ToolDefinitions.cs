using Azure.AI.Agents.Persistent;

namespace HospitalStaffMgmtApis.Agents.ToolDefinitions
{
    public static class ToolDefinitions
    {
        public static IReadOnlyList<FunctionToolDefinition> All => new[]
        {
            FindAvailableStaffTool.GetTool()
        };
    }
}
