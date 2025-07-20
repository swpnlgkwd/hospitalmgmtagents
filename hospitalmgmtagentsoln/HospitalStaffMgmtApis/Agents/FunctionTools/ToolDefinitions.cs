using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.Tools;

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
            ShiftSwapTool.GetTool()
        };
    }
}
