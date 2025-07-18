using HospitalStaffMgmtApis.Data.Models;
using System.Data.SqlClient;

namespace HospitalStaffMgmtApis.Data.Repository
{
    // Interface defining the contract for staff-related data operations
    public interface IStaffRepository
    {
        Task<List<FindStaffResult>> FindAvailableStaffAsync(FindStaffRequest request);
        Task<List<ShiftDay>> FetchShiftCalendar(ShiftCalendarRequest request);
        Task<string> ShiftSwapAsync(ShiftSwapRequest data);
        Task<string> CancelShiftAssignmentAsync(CancelShiftRequest data);
        Task<string> SubmitLeaveRequest(LeaveRequest leaveRequest);
        Task<List<ShiftRecord>> FetchStaffSchedule(StaffScheduleRequest request);
        Task<string> AssignShiftToStaff(AssignShiftRequest request);
    }

    // Implementation of IStaffRepository using SQL Server
    public class StaffRepository : IStaffRepository
    {
        private readonly string sqlConnectionString;

        // Constructor that initializes the repository with a SQL connection string
        public StaffRepository(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        // Finds available staff for a given date and shift who are not already assigned or on leave
        public async Task<List<FindStaffResult>> FindAvailableStaffAsync(FindStaffRequest request)
        {
            var result = new List<FindStaffResult>();

            using (var conn = new SqlConnection(sqlConnectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
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

                cmd.Parameters.AddWithValue("@shiftDate", request.ShiftDate);
                cmd.Parameters.AddWithValue("@shiftType", request.ShiftType);
                cmd.Parameters.AddWithValue("@role", (object?)request.Role ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@department", (object?)request.Department ?? DBNull.Value);

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

            return result;
        }

        // Fetches the shift calendar for a given date range
        public async Task<List<ShiftDay>> FetchShiftCalendar(ShiftCalendarRequest request)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                SELECT 
                    sa.shift_date, sa.shift_type,
                    s.name, s.role, s.department
                FROM ShiftAssignments sa
                INNER JOIN Staff s ON sa.staff_id = s.staff_id
                WHERE sa.shift_date BETWEEN @start AND @end
                ORDER BY sa.shift_date, sa.shift_type", conn);

            cmd.Parameters.AddWithValue("@start", request.StartDate);
            cmd.Parameters.AddWithValue("@end", request.EndDate);

            var reader = await cmd.ExecuteReaderAsync();
            var calendar = new Dictionary<string, List<StaffShift>>();

            while (await reader.ReadAsync())
            {
                string date = reader.GetDateTime(0).ToString("yyyy-MM-dd");
                string shiftType = reader.GetString(1);
                string name = reader.GetString(2);
                string role = reader.GetString(3);
                string dept = reader.GetString(4);

                if (!calendar.ContainsKey(date))
                    calendar[date] = new List<StaffShift>();

                calendar[date].Add(new StaffShift
                {
                    Name = name,
                    Role = role,
                    Department = dept,
                    ShiftType = shiftType
                });
            }

            return calendar.Select(kvp => new ShiftDay
            {
                ShiftDate = kvp.Key,
                Staff = kvp.Value
            }).ToList();
        }

        // Swaps a shift between two staff members after validating original assignment
        public async Task<string> ShiftSwapAsync(ShiftSwapRequest data)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            using var checkCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM ShiftAssignments
                WHERE staff_id = @originalId AND shift_date = @shiftDate AND shift_type = @shiftType", conn);

            checkCmd.Parameters.AddWithValue("@originalId", data.OriginalStaffId);
            checkCmd.Parameters.AddWithValue("@shiftDate", DateTime.Parse(data.ShiftDate));
            checkCmd.Parameters.AddWithValue("@shiftType", data.ShiftType);

            var exists = (int)await checkCmd.ExecuteScalarAsync();
            if (exists == 0)
            {
                return "Original shift assignment not found.";
            }

            using var tx = conn.BeginTransaction();

            try
            {
                using var deleteCmd = new SqlCommand(@"
                    DELETE FROM ShiftAssignments
                    WHERE staff_id = @originalId AND shift_date = @shiftDate AND shift_type = @shiftType", conn, tx);

                deleteCmd.Parameters.AddWithValue("@originalId", data.OriginalStaffId);
                deleteCmd.Parameters.AddWithValue("@shiftDate", DateTime.Parse(data.ShiftDate));
                deleteCmd.Parameters.AddWithValue("@shiftType", data.ShiftType);
                await deleteCmd.ExecuteNonQueryAsync();

                using var insertCmd = new SqlCommand(@"
                    INSERT INTO ShiftAssignments (staff_id, shift_date, shift_type)
                    VALUES (@replacementId, @shiftDate, @shiftType)", conn, tx);

                insertCmd.Parameters.AddWithValue("@replacementId", data.ReplacementStaffId);
                insertCmd.Parameters.AddWithValue("@shiftDate", DateTime.Parse(data.ShiftDate));
                insertCmd.Parameters.AddWithValue("@shiftType", data.ShiftType);
                await insertCmd.ExecuteNonQueryAsync();

                await tx.CommitAsync();
                return "Shift successfully reassigned to replacement staff.";
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // Cancels a staff's shift assignment
        public async Task<string> CancelShiftAssignmentAsync(CancelShiftRequest data)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var checkCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM ShiftAssignments
                WHERE staff_id = @staff_id AND shift_date = @shift_date AND shift_type = @shift_type", conn);

            checkCmd.Parameters.AddWithValue("@staff_id", data.StaffId);
            checkCmd.Parameters.AddWithValue("@shift_date", data.ShiftDate);
            checkCmd.Parameters.AddWithValue("@shift_type", data.ShiftType);

            int exists = (int)await checkCmd.ExecuteScalarAsync();

            if (exists == 0)
            {
                return "No such shift assignment found.";
            }

            var deleteCmd = new SqlCommand(@"
                DELETE FROM ShiftAssignments
                WHERE staff_id = @staff_id AND shift_date = @shift_date AND shift_type = @shift_type", conn);

            deleteCmd.Parameters.AddWithValue("@staff_id", data.StaffId);
            deleteCmd.Parameters.AddWithValue("@shift_date", data.ShiftDate);
            deleteCmd.Parameters.AddWithValue("@shift_type", data.ShiftType);

            await deleteCmd.ExecuteNonQueryAsync();

            return "Shift assignment cancelled.";
        }

        // Submits a leave request for a staff member
        public async Task<string> SubmitLeaveRequest(LeaveRequest leaveRequest)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                INSERT INTO LeaveRequests (staff_id, leave_start, leave_end, leave_type, status)
                VALUES (@staff_id, @leave_start, @leave_end, @leave_type, 'Pending')", conn);

            cmd.Parameters.AddWithValue("@staff_id", leaveRequest.StaffId);
            cmd.Parameters.AddWithValue("@leave_start", DateTime.Parse(leaveRequest.LeaveStart));
            cmd.Parameters.AddWithValue("@leave_end", DateTime.Parse(leaveRequest.LeaveEnd));
            cmd.Parameters.AddWithValue("@leave_type", leaveRequest.LeaveType);

            await cmd.ExecuteNonQueryAsync();

            return "Leave request submitted successfully.";
        }

        // Fetches the schedule for a specific staff member
        public async Task<List<ShiftRecord>> FetchStaffSchedule(StaffScheduleRequest request)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                SELECT shift_date, shift_type
                FROM ShiftAssignments
                WHERE staff_id = @staff_id
                ORDER BY shift_date", conn);

            cmd.Parameters.AddWithValue("@staff_id", request.StaffId);

            var reader = await cmd.ExecuteReaderAsync();
            var schedule = new List<ShiftRecord>();

            while (await reader.ReadAsync())
            {
                schedule.Add(new ShiftRecord
                {
                    ShiftDate = reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                    ShiftType = reader.GetString(1)
                });
            }

            return schedule;
        }

        // Assigns a shift to a staff member, ensuring no duplicates
        public async Task<string> AssignShiftToStaff(AssignShiftRequest request)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var checkCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM ShiftAssignments
                WHERE staff_id = @staff_id AND shift_date = @shift_date AND shift_type = @shift_type", conn);

            checkCmd.Parameters.AddWithValue("@staff_id", request.StaffId);
            checkCmd.Parameters.AddWithValue("@shift_date", DateTime.Parse(request.ShiftDate));
            checkCmd.Parameters.AddWithValue("@shift_type", request.ShiftType);

            int exists = (int)(await checkCmd.ExecuteScalarAsync());

            if (exists > 0)
            {
                return "Conflict: Staff is already assigned to this shift.";
            }

            var insertCmd = new SqlCommand(@"
                INSERT INTO ShiftAssignments (staff_id, shift_date, shift_type)
                VALUES (@staff_id, @shift_date, @shift_type)", conn);

            insertCmd.Parameters.AddWithValue("@staff_id", request.StaffId);
            insertCmd.Parameters.AddWithValue("@shift_date", DateTime.Parse(request.ShiftDate));
            insertCmd.Parameters.AddWithValue("@shift_type", request.ShiftType);

            await insertCmd.ExecuteNonQueryAsync();

            return "Shift assigned successfully.";
        }
    }
}
