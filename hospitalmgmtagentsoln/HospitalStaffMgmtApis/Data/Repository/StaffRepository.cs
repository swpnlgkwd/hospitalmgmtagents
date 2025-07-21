using Azure.Core;
using HospitalStaffMgmtApis.Data.Model;
using HospitalStaffMgmtApis.Data.Model.HospitalStaffMgmtApis.Models.Requests.HospitalStaffMgmtApis.Models.Requests;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using Microsoft.Recognizers.Text.DateTime;
using System.Data.SqlClient;
using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Repository
{

    // Implementation of staff repository using SQL Server
    public class StaffRepository : IStaffRepository
    {
        private readonly string sqlConnectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaffRepository"/> class.
        /// </summary>
        /// <param name="sqlConnectionString">The SQL connection string.</param>
        public StaffRepository(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        /// <summary>
        /// Resolves a list of staff members whose names partially match the input string.
        /// Matching is case-insensitive and accent-insensitive.
        /// </summary>
        /// <param name="namePart">Partial name to search for.</param>
        /// <returns>List of matching staff members with IDs and names.</returns>
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

        /// <summary>
        /// Resolves a department ID based on partial department name match.
        /// Matching is case-insensitive and accent-insensitive.
        /// </summary>
        /// <param name="departmentName">The partial name of the department.</param>
        /// <returns>Matching department ID if found, otherwise null.</returns>
        public async Task<int?> ResolveDepartmentIdAsync(string departmentName)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT department_id
        FROM Department
        WHERE name COLLATE Latin1_General_CI_AI LIKE @namePattern";
            cmd.Parameters.AddWithValue("@namePattern", $"%{departmentName?.Trim()}%");

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : (int?)null;
        }


        /// <summary>
        /// Fetches shift schedules filtered by optional staff ID, department ID, date range, and shift type.
        /// </summary>
        /// <param name="request">ShiftScheduleRequest containing filter criteria.</param>
        /// <returns>List of matching ShiftScheduleResponse objects.</returns>
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

            // Add SQL parameters
            cmd.Parameters.AddWithValue("@staffId", (object?)request.StaffId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@departmentId", (object?)request.DepartmentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@fromDate", request.FromDate.HasValue
                ? request.FromDate.Value.ToDateTime(TimeOnly.MinValue)
                : DBNull.Value);
            cmd.Parameters.AddWithValue("@toDate", request.ToDate.HasValue
                ? request.ToDate.Value.ToDateTime(TimeOnly.MinValue)
                : DBNull.Value);
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

        /// <summary>
        /// Applies for a leave for a staff member after validating against overlapping approved or pending requests.
        /// </summary>
        /// <param name="leaveRequest">Leave application details.</param>
        /// <returns>True if leave request was successfully submitted, otherwise false.</returns>
        public async Task<bool> ApplyForLeaveAsync(ApplyForLeaveRequest leaveRequest)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            // Check for overlapping leave entries
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
                return false; // Overlapping leave exists

            // Insert new leave request
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

        /// <summary>
        /// Automatically replaces shifts impacted by an approved leave with available staff.
        /// </summary>
        /// <param name="request">Request containing the leave period and staff ID.</param>
        /// <returns>Response with details of successful and unsuccessful shift replacements.</returns>
        public async Task<AutoReplaceShiftsForLeaveResponse> AutoReplaceShiftsForLeaveAsync(GetImpactedShiftsByLeaveRequest request)
        {
            var response = new AutoReplaceShiftsForLeaveResponse();

            // Step 1: Fetch shifts that are impacted by the given leave
            var impactedShifts = await FetchLeaveImpactedShiftsAsync(request);
            if (impactedShifts == null || !impactedShifts.Any())
            {
                // No shifts to replace
                return response;
            }

            // Step 2: Try replacing each impacted shift with available staff
            foreach (var impactedShift in impactedShifts)
            {
                var availableStaff = await FindAvailableStaffForShiftReplacementAsync(impactedShift);

                if (availableStaff.Any())
                {
                    // Currently using the first match (can be enhanced later)
                    var selected = availableStaff.First();

                    // Attempt to assign new staff to the impacted shift
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
                    // No staff available to replace the impacted shift
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

        /// <summary>
        /// Retrieves all shifts that are affected by a leave request for a particular staff member.
        /// </summary>
        /// <param name="request">Request containing the staff ID and leave date range.</param>
        /// <returns>List of impacted shifts.</returns>
        public async Task<List<LeaveImpactedShiftResponse>> FetchLeaveImpactedShiftsAsync(GetImpactedShiftsByLeaveRequest request)
        {
            var result = new List<LeaveImpactedShiftResponse>();

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT 
            sa.planned_shift_id AS shift_id,
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


        /// <summary>
        /// Finds staff available for assignment during a given date or range, based on department, role, and shift type.
        /// </summary>
        /// <param name="request">Request parameters including date range, department, role, and shift type.</param>
        /// <returns>List of available staff who match criteria.</returns>
        public async Task<List<FindAvailableStaffResponse>> FindAvailableStaffAsync(FindAvailableStaffRequest request)
        {
            var result = new List<FindAvailableStaffResponse>();

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();

            var startDate = request.FromDate;
            var endDate = request.ToDate ?? request.FromDate;

            var dateCondition = request.ToDate.HasValue
                ? "ps.shift_date BETWEEN @startDate AND @endDate"
                : "ps.shift_date = @startDate";

            cmd.CommandText = $@"
        SELECT DISTINCT
            s.staff_id,
            s.name AS staff_name,
            d.name AS department_name,
            r.role_name AS role_name
        FROM Staff s
        INNER JOIN Department d ON s.department_id = d.department_id
        INNER JOIN Role r ON s.role_id = r.role_id
        WHERE 
            s.is_active = 1
            {(request.DepartmentId.HasValue ? "AND d.department_id = @department" : "")}
            {(request.RoleId.HasValue ? "AND r.role_id = @role" : "")}

            -- Exclude staff explicitly marked as unavailable
            AND NOT EXISTS (
                SELECT 1
                FROM NurseAvailability a
                WHERE a.staff_id = s.staff_id
                  AND a.is_available = 0
                  AND a.available_date BETWEEN @startDate AND @endDate
                  {(string.IsNullOrWhiteSpace(request.ShiftType) ? "" : "AND a.shift_type_id = @shiftType")}
            )

            -- Exclude already assigned shifts
            AND NOT EXISTS (
                SELECT 1
                FROM PlannedShift ps
                WHERE ps.assigned_staff_id = s.staff_id
                  AND ps.shift_date BETWEEN @startDate AND @endDate
                  {(string.IsNullOrWhiteSpace(request.ShiftType) ? "" : "AND ps.shift_type_id = @shiftType")}
            )

            -- Exclude approved leaves
            AND NOT EXISTS (
                SELECT 1
                FROM LeaveRequests lr
                WHERE lr.staff_id = s.staff_id
                  AND lr.status = 'Approved'
                  AND lr.leave_start <= @endDate
                  AND lr.leave_end >= @startDate
            );
    ";

            cmd.Parameters.AddWithValue("@startDate", startDate);
            cmd.Parameters.AddWithValue("@endDate", endDate);

            if (request.DepartmentId.HasValue)
                cmd.Parameters.AddWithValue("@department", request.DepartmentId.Value);

            if (request.RoleId.HasValue)
                cmd.Parameters.AddWithValue("@role", request.RoleId.Value);

            if (!string.IsNullOrWhiteSpace(request.ShiftType))
            {
                var shiftTypeId = await GetShiftTypeIdByNameAsync(request.ShiftType);
                cmd.Parameters.AddWithValue("@shiftType", shiftTypeId);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new FindAvailableStaffResponse
                {
                    StaffId = reader.GetInt32(reader.GetOrdinal("staff_id")),
                    StaffName = reader.GetString(reader.GetOrdinal("staff_name")),
                    DepartmentName = reader.GetString(reader.GetOrdinal("department_name")),
                    RoleName = reader.GetString(reader.GetOrdinal("role_name")),
                    AvailableDate = request.FromDate,
                    ShiftType = request.ShiftType ?? ""
                });
            }

            return result;
        }

        /// <summary>
        /// Retrieves the list of all assigned shifts between the specified dates.
        /// </summary>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>
        /// A list of <see cref="ShiftScheduleResponse"/> objects representing the shift assignments.
        /// </returns>
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


        /// <summary>
        /// Retrieves the list of shifts assigned to a specific staff member within a date range.
        /// </summary>
        /// <param name="staffId">The ID of the staff member.</param>
        /// <param name="startDate">Start date of the shift range (inclusive).</param>
        /// <param name="endDate">End date of the shift range (inclusive).</param>
        /// <returns>List of <see cref="ShiftScheduleResponse"/> representing assigned shifts.</returns>
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

        /// <summary>
        /// Attempts to swap shifts between two staff members. The swap is allowed only if
        /// neither staff member is on leave or unavailable during the proposed shift.
        /// </summary>
        /// <param name="request">The swap shift request containing shift dates, types, and staff IDs.</param>
        /// <returns>True if swap is successful; false otherwise.</returns>
        public async Task<bool> SwapShiftsAsync(SwapShiftRequest request)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            /// <summary>
            /// Checks if the staff member is eligible for a given shift.
            /// </summary>
            async Task<bool> IsEligibleForShiftAsync(int staffId, DateTime shiftDate, int shiftTypeId)
            {
                // Check if staff is on approved/pending leave
                var leaveCheckCmd = new SqlCommand(@"
            SELECT COUNT(*) FROM LeaveRequests
            WHERE staff_id = @staff_id
              AND status != 'Rejected'
              AND @shift_date BETWEEN leave_start AND leave_end", conn, tx);

                leaveCheckCmd.Parameters.AddWithValue("@staff_id", staffId);
                leaveCheckCmd.Parameters.AddWithValue("@shift_date", shiftDate);

                var onLeave = (int)await leaveCheckCmd.ExecuteScalarAsync();
                if (onLeave > 0) return false;

                // Check NurseAvailability if explicitly marked unavailable (is_available = 0)
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
                    if (!reader.GetBoolean(0))
                    {
                        await reader.CloseAsync();
                        return false;
                    }
                }
                await reader.CloseAsync();

                return true;
            }

            // Fetch both shift IDs and confirm both are currently assigned to the given staff
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
                return false; // One or both shifts not found or not assigned correctly
            }

            // Validate if each staff can take the other's shift
            bool eligible1 = await IsEligibleForShiftAsync(request.StaffId1, request.ShiftDate2, request.ShiftTypeId2);
            bool eligible2 = await IsEligibleForShiftAsync(request.StaffId2, request.ShiftDate1, request.ShiftTypeId1);

            if (!eligible1 || !eligible2)
            {
                tx.Rollback();
                return false; // Leave or availability restriction
            }

            // Perform the swap by updating the assigned_staff_id
            var updateCmd = new SqlCommand(@"
        UPDATE PlannedShift SET assigned_staff_id = @new_staff1 WHERE planned_shift_id = @id1;
        UPDATE PlannedShift SET assigned_staff_id = @new_staff2 WHERE planned_shift_id = @id2;", conn, tx);

            updateCmd.Parameters.AddWithValue("@new_staff1", request.StaffId2); // Assign shift1 to staff2
            updateCmd.Parameters.AddWithValue("@id1", shiftId1.Value);

            updateCmd.Parameters.AddWithValue("@new_staff2", request.StaffId1); // Assign shift2 to staff1
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



        #region Helpers

        /// <summary>
        /// Retrieves the shift type ID from the database by its name.
        /// </summary>
        /// <param name="shiftTypeName">Name of the shift type (e.g., "Morning", "Evening").</param>
        /// <returns>
        /// The ID of the shift type if found; otherwise, <c>null</c>.
        /// </returns>
        private async Task<int?> GetShiftTypeIdByNameAsync(string? shiftTypeName)
        {
            if (string.IsNullOrWhiteSpace(shiftTypeName)) return null;

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT shift_type_id FROM ShiftType WHERE name = @name";
            cmd.Parameters.AddWithValue("@name", shiftTypeName);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? (int?)Convert.ToInt32(result) : null;
        }

        /// <summary>
        /// Retrieves the department ID from the database by its name.
        /// </summary>
        /// <param name="departmentName">The exact name of the department.</param>
        /// <returns>
        /// The ID of the department if found; otherwise, <c>null</c>.
        /// </returns>
        private async Task<int?> GetDepartmentIdByNameAsync(string departmentName)
        {
            if (string.IsNullOrWhiteSpace(departmentName)) return null;

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT department_id FROM Department WHERE name = @name";
            cmd.Parameters.AddWithValue("@name", departmentName);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? (int?)Convert.ToInt32(result) : null;
        }

        /// <summary>
        /// Finds available staff who are not assigned, not on leave, and marked available for a given shift.
        /// </summary>
        /// <param name="request">Shift details impacted by leave.</param>
        /// <returns>List of available staff for replacement.</returns>
        private async Task<List<FindStaffResult>> FindAvailableStaffForShiftReplacementAsync(LeaveImpactedShiftResponse request)
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


        /// <summary>
        /// Assigns a new staff member to a planned shift by updating the relevant record.
        /// </summary>
        /// <param name="request">Contains the staff ID and shift ID to update.</param>
        /// <returns>
        /// A message indicating the success or failure of the assignment.
        /// </returns>
        public async Task<string> AssignStaffToShiftAsync(AutoAssignShiftRequest request)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                // Update the PlannedShift table to assign the new staff member
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

        #endregion


    }
}

