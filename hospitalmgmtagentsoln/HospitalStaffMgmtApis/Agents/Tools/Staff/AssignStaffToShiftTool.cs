using System.Text.Json;
using Azure.AI.Agents.Persistent;

namespace HospitalStaffMgmtApis.Agents.Tools.Shifts
{
    public static class AssignStaffToShiftTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "assignStaffToShift",
                description: "Assigns a staff member to a shift, either by specifying the shiftId or by providing the shift date and shift type. Optionally, you can provide fromStaffId if this is a reassignment.",
                parameters: BinaryData.FromObjectAsJson(new
                {
                    type = "object",
                    properties = new
                    {
                        fromStaffId = new
                        {
                            type = "integer",
                            description = "Optional. The current staff member assigned to the shift, if it's a reassignment."
                        },
                        toStaffId = new
                        {
                            type = "integer",
                            description = "Required. The staff ID of the person to whom the shift should be assigned."
                        },
                        shiftDate = new
                        {
                            type = "string",
                            format = "date",
                            description = "The date of the shift (format: yyyy-MM-dd). Required if shiftId is not provided."
                        },
                        shiftType = new
                        {
                            type = "string",
                            description = "The type of the shift (e.g., Morning, Evening, Night). Required if shiftId is not provided."
                        },
                        shiftId = new
                        {
                            type = "integer",
                            description = "Optional. If known, provide the shiftId directly."
                        }
                    },
                    required = new[] { "toStaffId" }
                })
            );
        }
    }

}
