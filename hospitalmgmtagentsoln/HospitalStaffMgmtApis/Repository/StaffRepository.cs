using HospitalStaffMgmtApis.Models;
using System.Data.SqlClient;

namespace HospitalStaffMgmtApis.Functions
{
    // Interface defining the contract for staff repository
    public interface IStaffRepository
    {
        Task<List<FindStaffResult>> FindAvailableStaffAsync(FindStaffRequest request);
    }

    // Concrete implementation of the IStaffRepository for interacting with SQL Server
    public class StaffRepository : IStaffRepository
    {
        private readonly string sqlConnectionString;

        // Constructor accepting the SQL connection string
        public StaffRepository(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        // Fetches a list of available staff based on role, department, date, and shift
        public async Task<List<FindStaffResult>> FindAvailableStaffAsync(FindStaffRequest request)
        {
            var result = new List<FindStaffResult>();

            // Open SQL connection
            using (var conn = new SqlConnection(sqlConnectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();

                // SQL query to find available staff who are not assigned to a shift or on approved leave
                cmd.CommandText = @"
                    SELECT s.staff_id, s.name, s.role, s.department, s.specialty
                    FROM Staff s
                    WHERE s.is_active = 1
                      AND (@role IS NULL OR s.role = @role)
                      AND (@department IS NULL OR s.department = @department)
                      AND s.staff_id NOT IN (
                          SELECT sa.staff_id
                          FROM ShiftAssignments sa
                          WHERE sa.shift_date = @shiftDate
                            AND sa.shift_type = @shiftType
                      )
                      AND s.staff_id NOT IN (
                          SELECT lr.staff_id
                          FROM LeaveRequests lr
                          WHERE lr.status = 'Approved'
                            AND @shiftDate BETWEEN lr.leave_start AND lr.leave_end
                      )";

                // Add parameters safely to prevent SQL injection
                cmd.Parameters.AddWithValue("@shiftDate", request.ShiftDate);
                cmd.Parameters.AddWithValue("@shiftType", request.ShiftType);
                cmd.Parameters.AddWithValue("@role", (object?)request.Role ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@department", (object?)request.Department ?? DBNull.Value);

                // Execute the query and read results
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new FindStaffResult
                    {
                        StaffId = reader.GetInt32(reader.GetOrdinal("staff_id")),
                        Name = reader.IsDBNull(reader.GetOrdinal("name")) ? null : reader.GetString(reader.GetOrdinal("name")),
                        Role = reader.IsDBNull(reader.GetOrdinal("role")) ? null : reader.GetString(reader.GetOrdinal("role")),
                        Department = reader.IsDBNull(reader.GetOrdinal("department")) ? null : reader.GetString(reader.GetOrdinal("department")),
                        Specialty = reader.IsDBNull(reader.GetOrdinal("specialty")) ? null : reader.GetString(reader.GetOrdinal("specialty"))
                    });
                }
            }

            // Return the final list of available staff
            return result;
        }
    }
}
