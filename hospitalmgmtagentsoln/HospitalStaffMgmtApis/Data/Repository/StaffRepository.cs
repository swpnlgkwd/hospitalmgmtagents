using Azure.Core;
using HospitalStaffMgmtApis.Data.Model;
using Microsoft.Recognizers.Text.DateTime;
using System.Data.SqlClient;
using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Repository
{
    // Interface defining the contract for staff-related data operations
    public interface IStaffRepository
    {
        // Resolve staff names based on partial input (for auto-suggest/autocomplete)
        Task<List<StaffIdResult>> ResolveStaffNameAsync(string namePart);

        // Fetch shift schedule for a specific staff member or department
        Task<List<ShiftScheduleResponse>> GetShiftScheduleAsync(ShiftScheduleRequest shiftScheduleRequest);

        // Submit leave request for a staff member
        Task<bool> ApplyForLeaveAsync(ApplyForLeaveRequest request);

        // Auto-replace shifts impacted due to leave by assigning alternative staff
        Task<AutoReplaceShiftsForLeaveResponse> AutoReplaceShiftsForLeaveAsync(GetImpactedShiftsByLeaveRequest request);

        Task<List<ShiftScheduleResponse>> GetShiftScheduleBetweenDatesAsync(DateOnly startDate, DateOnly endDate);
        Task<bool> SwapShiftsAsync(SwapShiftRequest request);
        Task<List<ShiftScheduleResponse>> FetchShiftInformationByStaffId(int staffId, DateOnly startDate, DateOnly endDate);


       // Task FetchCoverageByDateRangeAsync(Coverage coverage);

    }

    // Implementation of staff repository using SQL Server
    public class StaffRepository : IStaffRepository
    {
        private readonly string sqlConnectionString;

        // Constructor initializing SQL connection string
        public StaffRepository(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        // Resolve staff names by partial matching (case-insensitive and accent-insensitive)
        public async Task<List<StaffIdResult>> ResolveStaffNameAsync(string namePart)
        {
            var results = new List<StaffIdResult>();

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT TOP 10 staff_id, name
                FROM Staff
                WHERE is_active = 1
                  AND name COLLATE Latin1_General_CI_AI LIKE @namePattern
                ORDER BY name";
            cmd.Parameters.AddWithValue("@namePattern", $"%{namePart?.Trim()}%");

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new StaffIdResult
                {
                    StaffId = reader.GetInt32(reader.GetOrdinal("staff_id")),
                    Name = reader.GetString(reader.GetOrdinal("name"))
                });
            }

            return results;
        }

        // Fetch shift schedule based on optional staffId, departmentId, date range and shift type
        public async Task<List<ShiftScheduleResponse>> GetShiftScheduleAsync(ShiftScheduleRequest request)
        {
            var result = new List<ShiftScheduleResponse>();

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    sa.shift_date,
                    st.name AS shift_type,
                    d.name AS department_name,
                    s.name AS staff_name,
                    r.role_name AS role
                FROM PlannedShift sa
                INNER JOIN Staff s ON sa.assigned_staff_id = s.staff_id
                INNER JOIN ShiftType st ON sa.shift_type_id = st.shift_type_id
                INNER JOIN Department d ON s.department_id = d.department_id
                INNER JOIN Role r ON s.role_id = r.role_id
                WHERE 
                    (@staffId IS NULL OR s.staff_id = @staffId)
                    AND (@departmentId IS NULL OR d.department_id = @departmentId)
                    AND (@fromDate IS NULL OR sa.shift_date >= @fromDate)
                    AND (@toDate IS NULL OR sa.shift_date <= @toDate)
                    AND (@shiftType IS NULL OR st.name = @shiftType)
                ORDER BY sa.shift_date ASC, st.start_time ASC";

            // Add parameters
            cmd.Parameters.AddWithValue("@staffId", (object?)request.StaffId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@departmentId", (object?)request.DepartmentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@fromDate", request.FromDate.HasValue ? request.FromDate.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            cmd.Parameters.AddWithValue("@toDate", request.ToDate.HasValue ? request.ToDate.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
            cmd.Parameters.AddWithValue("@shiftType", (object?)request.ShiftType ?? DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new ShiftScheduleResponse
                {
                    ShiftDate = reader.GetDateTime(reader.GetOrdinal("shift_date")),
                    ShiftType = reader.GetString(reader.GetOrdinal("shift_type")),
                    DepartmentName = reader.GetString(reader.GetOrdinal("department_name")),
                    StaffName = reader.GetString(reader.GetOrdinal("staff_name")),
                    Role = reader.GetString(reader.GetOrdinal("role"))
                });
            }

            return result;
        }

        // Submit leave request after checking for overlapping approved/pending requests
        public async Task<bool> ApplyForLeaveAsync(ApplyForLeaveRequest leaveRequest)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            // Check for overlap
            var checkCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM LeaveRequests
                WHERE staff_id = @staff_id
                  AND status != 'Rejected'
                  AND (
                        (@leave_start BETWEEN leave_start AND leave_end)
                        OR (@leave_end BETWEEN leave_start AND leave_end)
                        OR (leave_start BETWEEN @leave_start AND @leave_end)
                     )", conn);

            checkCmd.Parameters.AddWithValue("@staff_id", leaveRequest.StaffId);
            checkCmd.Parameters.AddWithValue("@leave_start", leaveRequest.LeaveStart);
            checkCmd.Parameters.AddWithValue("@leave_end", leaveRequest.LeaveEnd);

            int overlapCount = (int)await checkCmd.ExecuteScalarAsync();
            if (overlapCount > 0)
                return false; // Overlap found

            // Insert leave request
            var cmd = new SqlCommand(@"
                INSERT INTO LeaveRequests (staff_id, leave_start, leave_end, leave_type, status)
                VALUES (@staff_id, @leave_start, @leave_end, @leave_type, 'Pending')", conn);

            cmd.Parameters.AddWithValue("@staff_id", leaveRequest.StaffId);
            cmd.Parameters.AddWithValue("@leave_start", leaveRequest.LeaveStart);
            cmd.Parameters.AddWithValue("@leave_end", leaveRequest.LeaveEnd);
            cmd.Parameters.AddWithValue("@leave_type", leaveRequest.LeaveType);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Fetch impacted shifts for a leave and attempt auto-replacement with available staff
        public async Task<AutoReplaceShiftsForLeaveResponse> AutoReplaceShiftsForLeaveAsync(GetImpactedShiftsByLeaveRequest request)
        {
            var response = new AutoReplaceShiftsForLeaveResponse();


            var impactedShifts = await FetchLeaveImpactedShiftsAsync(request);
            if (impactedShifts == null || !impactedShifts.Any())
            {
                // No shifts to replace, return early
                return response;
            }

            foreach (var impactedShift in impactedShifts)
            {
                var availableStaff = await FindAvailableStaffForShiftReplacementAsync(impactedShift);
                if (availableStaff.Any())
                {
                    var selected = availableStaff.First(); // TODO: Replace with smarter selection

                    var assignResult = await AssignStaffToShiftAsync(new AutoAssignShiftRequest
                    {
                        StaffId = selected.StaffId,
                        ShiftDate = impactedShift.ShiftDate.ToString("yyyy-MM-dd"),
                        ShiftType = impactedShift.ShiftType,
                        ShiftId = impactedShift.ShiftId
                    });

                    response.AssignedShifts.Add(new AutoReplaceShiftsForLeaveResult
                    {
                        ShiftDate = impactedShift.ShiftDate,
                        ShiftType = impactedShift.ShiftType,
                        Department = impactedShift.Department,
                        Role = impactedShift.Role,
                        AssignedTo = selected.Name,
                        Success = assignResult == "Shift reassigned successfully.",
                        Message = assignResult
                    });
                }
                else
                {
                    response.UnassignedShifts.Add(new AutoReplaceShiftsForLeaveResult
                    {
                        ShiftDate = impactedShift.ShiftDate,
                        ShiftType = impactedShift.ShiftType,
                        Department = impactedShift.Department,
                        Role = impactedShift.Role,
                        Success = false,
                        Message = "No available replacement found."
                    });
                }
            }

            return response;
        }

        // Fetch all shift assignments affected by leave for a staff member within a date range
        public async Task<List<LeaveImpactedShiftResponse>> FetchLeaveImpactedShiftsAsync(GetImpactedShiftsByLeaveRequest request)
        {
            var result = new List<LeaveImpactedShiftResponse>();

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    sa.planned_shift_id as shift_id,
                    sa.shift_date,
                    st.name AS shift_type,
                    d.name AS department_name,
                    r.role_name AS role
                FROM PlannedShift sa
                INNER JOIN Staff s ON sa.assigned_staff_id = s.staff_id
                INNER JOIN ShiftType st ON sa.shift_type_id = st.shift_type_id
                INNER JOIN Department d ON s.department_id = d.department_id
                INNER JOIN Role r ON s.role_id = r.role_id
                WHERE 
                    s.staff_id = @staffId
                    AND sa.shift_date BETWEEN @fromDate AND @toDate
                ORDER BY sa.shift_date ASC, st.start_time ASC";

            cmd.Parameters.AddWithValue("@staffId", request.StaffId);
            cmd.Parameters.AddWithValue("@fromDate", request.FromDate);
            cmd.Parameters.AddWithValue("@toDate", request.ToDate);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new LeaveImpactedShiftResponse
                {
                    ShiftId = reader.GetInt32(reader.GetOrdinal("shift_id")),
                    ShiftDate = reader.GetDateTime(reader.GetOrdinal("shift_date")),
                    ShiftType = reader.GetString(reader.GetOrdinal("shift_type")),
                    Department = reader.GetString(reader.GetOrdinal("department_name")),
                    Role = reader.GetString(reader.GetOrdinal("role"))
                });
            }

            return result;
        }

        // Utility method to get department ID by name
        private async Task<int?> GetDepartmentIdByNameAsync(string departmentName)
        {
            if (string.IsNullOrWhiteSpace(departmentName)) return null;

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT department_id FROM Department WHERE name = @name";
            cmd.Parameters.AddWithValue("@name", departmentName);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? (int?)Convert.ToInt32(result) : null;
        }

        // Utility method to get shift type ID by name
        private async Task<int?> GetShiftTypeIdByNameAsync(string? shiftTypeName)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();
            if (string.IsNullOrWhiteSpace(shiftTypeName)) return null;

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT shift_type_id FROM ShiftType WHERE name = @name";
            cmd.Parameters.AddWithValue("@name", shiftTypeName);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? (int?)Convert.ToInt32(result) : null;
        }

        // Finds available staff who are not assigned, not on leave, and marked available
        public async Task<List<FindStaffResult>> FindAvailableStaffForShiftReplacementAsync(LeaveImpactedShiftResponse request)
        {
            var result = new List<FindStaffResult>();

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT 
                s.staff_id, 
                s.name, 
                d.name AS department
            FROM Staff s
            JOIN Department d ON s.department_id = d.department_id
            WHERE 
                s.is_active = 1

                -- Exclude staff who have explicitly said they are NOT available for this date & shift
                AND NOT EXISTS (
                    SELECT 1 
                    FROM NurseAvailability a
                    WHERE a.staff_id = s.staff_id
                      AND a.available_date = @shiftDate
                      AND a.shift_type_id = @shiftType
                      AND a.is_available = 0
                )

                -- Exclude staff who are already assigned to this shift on that date
                AND NOT EXISTS (
                    SELECT 1 
                    FROM PlannedShift sa
                    WHERE sa.assigned_staff_id = s.staff_id 
                      AND sa.shift_date = @shiftDate 
                      AND sa.shift_type_id = @shiftType
                )

                -- Exclude staff who are on approved leave on this date
                AND NOT EXISTS (
                    SELECT 1 
                    FROM LeaveRequests lr
                    WHERE lr.staff_id = s.staff_id 
                      AND lr.status = 'Approved'
                      AND @shiftDate BETWEEN lr.leave_start AND lr.leave_end
                );
            ";

            var departmentId = await GetDepartmentIdByNameAsync(request.Department);
            var shiftTypeID = await GetShiftTypeIdByNameAsync(request.ShiftType);

            cmd.Parameters.AddWithValue("@department", (object?)departmentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@shiftDate", request.ShiftDate);
            cmd.Parameters.AddWithValue("@shiftType", shiftTypeID);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new FindStaffResult
                {
                    StaffId = reader.GetInt32(reader.GetOrdinal("staff_id")),
                    Name = reader["name"] as string ?? "",
                    Department = reader["department"] as string ?? ""
                });
            }

            return result;
        }

        // Assign new staff to a shift (updates ShiftAssignments and PlannedShift)
        public async Task<string> AssignStaffToShiftAsync(AutoAssignShiftRequest request)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {                

                // Update PlannedShift (if applicable)
                var updatePlannedCmd = new SqlCommand(@"
                    UPDATE PlannedShift 
                    SET assigned_staff_id = @newStaffId 
                    WHERE planned_shift_id = @shiftId", conn, transaction);

                updatePlannedCmd.Parameters.AddWithValue("@newStaffId", request.StaffId);
                updatePlannedCmd.Parameters.AddWithValue("@shiftId", request.ShiftId);

                await updatePlannedCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return "Shift reassigned successfully.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return $"Error during reassignment: {ex.Message}";
            }
        }

        public async Task<List<ShiftScheduleResponse>> GetShiftScheduleBetweenDatesAsync(DateOnly startDate, DateOnly endDate)
        {
            var result = new List<ShiftScheduleResponse>();

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT 
            sa.shift_date,
            st.name AS shift_type,
            d.name AS department_name,
            s.name AS staff_name,
            r.role_name AS role
        FROM PlannedShift sa
        INNER JOIN Staff s ON sa.assigned_staff_id = s.staff_id
        INNER JOIN ShiftType st ON sa.shift_type_id = st.shift_type_id
        INNER JOIN Department d ON s.department_id = d.department_id
        INNER JOIN Role r ON s.role_id = r.role_id
        WHERE 
            sa.shift_date >= @startDate AND sa.shift_date <= @endDate
        ORDER BY sa.shift_date ASC, st.start_time ASC";

            cmd.Parameters.AddWithValue("@startDate", startDate.ToDateTime(TimeOnly.MinValue));
            cmd.Parameters.AddWithValue("@endDate", endDate.ToDateTime(TimeOnly.MaxValue));

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new ShiftScheduleResponse
                {
                    ShiftDate = reader.GetDateTime(reader.GetOrdinal("shift_date")),
                    ShiftType = reader.GetString(reader.GetOrdinal("shift_type")),
                    DepartmentName = reader.GetString(reader.GetOrdinal("department_name")),
                    StaffName = reader.GetString(reader.GetOrdinal("staff_name")),
                    Role = reader.GetString(reader.GetOrdinal("role"))
                });
            }

            return result;
        }

        public async Task<List<ShiftScheduleResponse>> FetchShiftInformationByStaffId(int staffId, DateOnly startDate, DateOnly endDate)
        {
            var result = new List<ShiftScheduleResponse>();

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT 
            sa.shift_date,
            st.name AS shift_type,
            d.name AS department_name,
            s.name AS staff_name,
            r.role_name AS role
        FROM PlannedShift sa
        INNER JOIN Staff s ON sa.assigned_staff_id = s.staff_id
        INNER JOIN ShiftType st ON sa.shift_type_id = st.shift_type_id
        INNER JOIN Department d ON s.department_id = d.department_id
        INNER JOIN Role r ON s.role_id = r.role_id
        WHERE 
            sa.shift_date >= @startDate 
            AND sa.shift_date <= @endDate
            AND sa.assigned_staff_id = @staffId
        ORDER BY sa.shift_date ASC, st.start_time ASC";

            cmd.Parameters.AddWithValue("@startDate", startDate.ToDateTime(TimeOnly.MinValue));
            cmd.Parameters.AddWithValue("@endDate", endDate.ToDateTime(TimeOnly.MaxValue));
            cmd.Parameters.AddWithValue("@staffId", staffId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new ShiftScheduleResponse
                {
                    ShiftDate = reader.GetDateTime(reader.GetOrdinal("shift_date")),
                    ShiftType = reader.GetString(reader.GetOrdinal("shift_type")),
                    DepartmentName = reader.GetString(reader.GetOrdinal("department_name")),
                    StaffName = reader.GetString(reader.GetOrdinal("staff_name")),
                    Role = reader.GetString(reader.GetOrdinal("role"))
                });
            }

            return result;
        }


        public async Task<bool> SwapShiftsAsync(SwapShiftRequest request)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            // Helper method to validate a swap
            async Task<bool> IsEligibleForShiftAsync(int staffId, DateTime shiftDate, int shiftTypeId)
            {
                // 1. Check LeaveRequests
                var leaveCheckCmd = new SqlCommand(@"
            SELECT COUNT(*) FROM LeaveRequests
            WHERE staff_id = @staff_id
              AND status != 'Rejected'
              AND @shift_date BETWEEN leave_start AND leave_end", conn, tx);

                leaveCheckCmd.Parameters.AddWithValue("@staff_id", staffId);
                leaveCheckCmd.Parameters.AddWithValue("@shift_date", shiftDate);

                var onLeave = (int)await leaveCheckCmd.ExecuteScalarAsync();
                if (onLeave > 0) return false;

                // 2. Check NurseAvailability (only if entry exists and is_available = 0)
                var availCheckCmd = new SqlCommand(@"
            SELECT is_available FROM NurseAvailability
            WHERE staff_id = @staff_id AND available_date = @shift_date
              AND (shift_type_id IS NULL OR shift_type_id = @shift_type_id)", conn, tx);

                availCheckCmd.Parameters.AddWithValue("@staff_id", staffId);
                availCheckCmd.Parameters.AddWithValue("@shift_date", shiftDate);
                availCheckCmd.Parameters.AddWithValue("@shift_type_id", shiftTypeId);

                var reader = await availCheckCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (!reader.GetBoolean(0)) // is_available = 0
                    {
                        await reader.CloseAsync();
                        return false;
                    }
                }
                await reader.CloseAsync();

                return true;
            }

            // 3. Fetch original shift records and ensure staff are actually assigned
            var fetchCmd = new SqlCommand(@"
        SELECT planned_shift_id FROM PlannedShift
        WHERE shift_date = @date1 AND shift_type_id = @type1 AND assigned_staff_id = @staff1;
        
        SELECT planned_shift_id FROM PlannedShift
        WHERE shift_date = @date2 AND shift_type_id = @type2 AND assigned_staff_id = @staff2;", conn, tx);

            fetchCmd.Parameters.AddWithValue("@date1", request.ShiftDate1);
            fetchCmd.Parameters.AddWithValue("@type1", request.ShiftTypeId1);
            fetchCmd.Parameters.AddWithValue("@staff1", request.StaffId1);

            fetchCmd.Parameters.AddWithValue("@date2", request.ShiftDate2);
            fetchCmd.Parameters.AddWithValue("@type2", request.ShiftTypeId2);
            fetchCmd.Parameters.AddWithValue("@staff2", request.StaffId2);

            int? shiftId1 = null, shiftId2 = null;
            using (var reader = await fetchCmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                    shiftId1 = reader.GetInt32(0);
                await reader.NextResultAsync();
                if (await reader.ReadAsync())
                    shiftId2 = reader.GetInt32(0);
            }

            if (shiftId1 == null || shiftId2 == null)
            {
                tx.Rollback();
                return false; // One of the shifts not found or not assigned correctly
            }

            // 4. Validate new assignments
            bool eligible1 = await IsEligibleForShiftAsync(request.StaffId1, request.ShiftDate2, request.ShiftTypeId2);
            bool eligible2 = await IsEligibleForShiftAsync(request.StaffId2, request.ShiftDate1, request.ShiftTypeId1);

            if (!eligible1 || !eligible2)
            {
                tx.Rollback();
                return false; // Swap violates availability or leave policy
            }

            // 5. Perform the swap
            var updateCmd = new SqlCommand(@"
        UPDATE PlannedShift SET assigned_staff_id = @new_staff1 WHERE planned_shift_id = @id1;
        UPDATE PlannedShift SET assigned_staff_id = @new_staff2 WHERE planned_shift_id = @id2;", conn, tx);

            updateCmd.Parameters.AddWithValue("@new_staff1", request.StaffId2); // staff2 now gets shift1
            updateCmd.Parameters.AddWithValue("@id1", shiftId1.Value);
            updateCmd.Parameters.AddWithValue("@new_staff2", request.StaffId1); // staff1 now gets shift2
            updateCmd.Parameters.AddWithValue("@id2", shiftId2.Value);

            int rows = await updateCmd.ExecuteNonQueryAsync();

            if (rows >= 2)
            {
                tx.Commit();
                return true;
            }
            else
            {
                tx.Rollback();
                return false;
            }
        }

        //public async Task<List<ShiftScheduleResponse>> FetchCoverageByDateRangeAsync(Coverage request)
        //{
        //    var result = new List<ShiftScheduleResponse>();

        //    using var conn = new SqlConnection(sqlConnectionString);
        //    await conn.OpenAsync();

        //    var cmd = conn.CreateCommand();
        //    cmd.CommandText = @"
        //SELECT 
        //    sa.shift_date,
        //    st.name AS shift_type,
        //    d.name AS department_name,
        //    s.name AS staff_name,
        //    r.role_name AS role
        //FROM PlannedShift sa
        //INNER JOIN Staff s ON sa.assigned_staff_id = s.staff_id
        //INNER JOIN ShiftType st ON sa.shift_type_id = st.shift_type_id
        //INNER JOIN Department d ON s.department_id = d.department_id
        //INNER JOIN Role r ON s.role_id = r.role_id
        //WHERE 
        //    sa.shift_date >= @startDate AND sa.shift_date <= @endDate
        //ORDER BY sa.shift_date ASC, st.start_time ASC";

        //    cmd.Parameters.AddWithValue("@startDate", startDate.ToDateTime(TimeOnly.MinValue));
        //    cmd.Parameters.AddWithValue("@endDate", endDate.ToDateTime(TimeOnly.MaxValue));

        //    using var reader = await cmd.ExecuteReaderAsync();
        //    while (await reader.ReadAsync())
        //    {
        //        result.Add(new ShiftScheduleResponse
        //        {
        //            ShiftDate = reader.GetDateTime(reader.GetOrdinal("shift_date")),
        //            ShiftType = reader.GetString(reader.GetOrdinal("shift_type")),
        //            DepartmentName = reader.GetString(reader.GetOrdinal("department_name")),
        //            StaffName = reader.GetString(reader.GetOrdinal("staff_name")),
        //            Role = reader.GetString(reader.GetOrdinal("role"))
        //        });
        //    }

        //    return result;
        //}


    }
}

