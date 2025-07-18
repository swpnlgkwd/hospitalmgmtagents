using Azure.AI.Agents.Persistent;

namespace HospitalStaffMgmtApis.Agents.Tools
{
    public static class ToolDefinitions
    {
        public static IReadOnlyList<FunctionToolDefinition> All => new[]
        {
            FindAvailableStaffTool.GetTool(),
            FetchShiftCalendarTool.GetTool(),
            ShiftSwapTool.GetTool(),
            CancelShiftAssignmentTool.GetTool(),
            SubmitLeaveRequestTool.GetTool(),
            FetchStaffScheduleTool.GetTool(),
            AssignShiftToStaffTool.GetTool(),
        };
    }
}
