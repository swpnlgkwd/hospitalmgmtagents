using Azure.AI.Agents.Persistent;

namespace HospitalStaffMgmtApis.Agents.FunctionTools
{
    public static class ToolDefinitions
    {
        public static IReadOnlyList<FunctionToolDefinition> All => new[]
        {
            GetShiftScheduleTool.GetTool(),
            StaffNameResolverTool.GetTool(),
            ApplyForLeaveTool.GetTool(),
            AutoReplaceShiftsForLeaveTool.GetTool(),
            ResolveRelativeDateTool.GetTool(),
            //FindAvailableStaffTool.GetTool(),
            //FetchShiftCalendarTool.GetTool(),
            //ShiftSwapTool.GetTool(),
            //CancelShiftAssignmentTool.GetTool(),
            //SubmitLeaveRequestTool.GetTool(),
            //FetchStaffScheduleTool.GetTool(),
            //AssignShiftToStaffTool.GetTool(),
        };
    }
}
