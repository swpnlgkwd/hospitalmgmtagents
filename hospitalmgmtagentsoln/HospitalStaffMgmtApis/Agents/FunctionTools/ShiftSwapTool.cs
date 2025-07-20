using Azure.AI.Agents.Persistent;
using System.Text.Json;

namespace HospitalStaffMgmtApis.Agents.FunctionTools
{
    public static class ShiftSwapTool
    {
        public static FunctionToolDefinition GetTool()
        {
            return new FunctionToolDefinition(
                name: "swapShiftsBetweenEmployees",
                description: "Use this tool when two employees want to swap their shifts. This reassigns their respective shifts to each other after validating eligibility and avoiding conflicts.",
                parameters: BinaryData.FromObjectAsJson(
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            staffId1 = new
                            {
                                type = "integer",
                                description = "The ID of the first staff member who wants to swap the shift."
                            },
                            shiftDate1 = new
                            {
                                type = "string",
                                format = "date",
                                description = "The date of the first staff member's shift (in yyyy-MM-dd format)."
                            },
                            shiftTypeId1 = new
                            {
                                type = "integer",
                                description = "The shift type ID for the first staff member (e.g., Morning = 1)."
                            },
                            staffId2 = new
                            {
                                type = "integer",
                                description = "The ID of the second staff member who will swap shifts with the first."
                            },
                            shiftDate2 = new
                            {
                                type = "string",
                                format = "date",
                                description = "The date of the second staff member's shift (in yyyy-MM-dd format)."
                            },
                            shiftTypeId2 = new
                            {
                                type = "integer",
                                description = "The shift type ID for the second staff member (e.g., Night = 3)."
                            }
                        },
                        required = new[]
                        {
                            "staffId1",
                            "shiftDate1",
                            "shiftTypeId1",
                            "staffId2",
                            "shiftDate2",
                            "shiftTypeId2"
                        }
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                )
            );
        }
    }
}
