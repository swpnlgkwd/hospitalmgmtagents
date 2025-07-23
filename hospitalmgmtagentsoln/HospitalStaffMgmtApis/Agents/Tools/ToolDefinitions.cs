using Azure.AI.Agents.Persistent;
using HospitalStaffMgmtApis.Agents.Tools.Department;
using HospitalStaffMgmtApis.Agents.Tools.Helpers;
using HospitalStaffMgmtApis.Agents.Tools.HospitalStaffMgmtApis.Agents.Tools;
using HospitalStaffMgmtApis.Agents.Tools.Leave;
using HospitalStaffMgmtApis.Agents.Tools.Shift;
using HospitalStaffMgmtApis.Agents.Tools.Staff;

namespace HospitalStaffMgmtApis.Agents.Tools
{
    public static class ToolDefinitions
    {
        public static IReadOnlyList<FunctionToolDefinition> All => new[]
        {
            ResolveRelativeDateTool.GetTool(),
            DepartmentNameResolverTool.GetTool(),
            GetShiftScheduleTool.GetTool(),
            StaffNameResolverTool.GetTool(),
            FindAvailableStaffTool.GetTool(),
            ViewPendingLeaveRequestsTool.GetTool(),
            UncoverShiftsTool.GetTool()
            //ApplyForLeaveTool.GetTool(),
            //AutoReplaceShiftsForLeaveTool.GetTool(),

            //ShiftSwapTool.GetTool(),
            //UncoverShiftsTool.GetTool(),
            //ViewPendingLeaveRequestsTool.GetTool()
        };
    }
}
