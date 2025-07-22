using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.FunctionTools;
using HospitalStaffMgmtApis.Agents.Tools.HospitalStaffMgmtApis.Agents.Tools;

namespace HospitalStaffMgmtApis.Agents.Tools
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
            ShiftSwapTool.GetTool(),
            DepartmentNameResolverTool.GetTool(),
            FindAvailableStaffTool.GetTool(),
            UncoverShiftsTool.GetTool()
        };
    }
}
