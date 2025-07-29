using Azure.AI.Agents.Persistent;
using HospitalSchedulingApp.Agent.Tools.Department;
using HospitalSchedulingApp.Agent.Tools.HelperTools;
using HospitalSchedulingApp.Agent.Tools.Shift;
using HospitalSchedulingApp.Agent.Tools.Staff;

namespace HospitalSchedulingApp.Agent.Tools
{
    public static class ToolDefinitions
    {
        public static IReadOnlyList<FunctionToolDefinition> All => new[]
        {
            FilterShiftScheduleTool.GetTool(),
            ResolveDepartmentInfoTool.GetTool(),
            ResolveStaffInfoByNameTool.GetTool(),
            ResolveRelativeDateTool.GetTool(),
            SearchAvailableStaffTool.GetTool(),
           
             //ResolveRelativeDateTool.GetTool(),
            //DepartmentNameResolverTool.GetTool(),
            //GetShiftScheduleTool.GetTool(),
            //StaffNameResolverTool.GetTool(),
            //FindAvailableStaffTool.GetTool(),
            //ViewPendingLeaveRequestsTool.GetTool(),
            //UncoverShiftsTool.GetTool(),
            //ApplyForLeaveTool.GetTool(),
            //ApproveOrRejectLeaveRequestTool.GetTool(),
            //AssignStaffToShiftTool.GetTool(),
            //ResolveStaffReferenceTool.GetTool()
            
            //AssignStaffToShiftTool.GetTool(),
            //AutoReplaceShiftsForLeaveTool.GetTool(),

            //ShiftSwapTool.GetTool(),
            //UncoverShiftsTool.GetTool(),
            //ViewPendingLeaveRequestsTool.GetTool()
        };
    }
}
