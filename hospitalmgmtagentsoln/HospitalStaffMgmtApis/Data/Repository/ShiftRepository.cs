using Azure.Core;
using HospitalStaffMgmtApis.Data.Models;
using HospitalStaffMgmtApis.Data.Models.Shift;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using HospitalStaffMgmtApis.Models.Requests;
using System.Data;
using System.Data.SqlClient;

namespace HospitalStaffMgmtApis.Data.Repository
{
    public class ShiftRepository : IShiftRepository
    {
        private readonly string sqlConnectionString;
        /// <summary>
        /// Initializes a new instance of the <see cref="StaffRepository"/> class.
        /// </summary>
        /// <param name="sqlConnectionString">The SQL connection string.</param>
        public ShiftRepository(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
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
            d.department_id,
            d.name AS department_name,
            s.name AS staff_name,
            r.role_name AS role,
            r.role_id,
            sa.assigned_staff_id AS staff_id,
            ss.status_name as shift_status
        FROM PlannedShift sa       
        INNER JOIN ShiftType st ON sa.shift_type_id = st.shift_type_id
        INNER JOIN ShiftStatus ss ON sa.shift_status_id = ss.shift_status_id
        INNER JOIN Department d ON sa.department_id = d.department_id
        LEFT OUTER JOIN Staff s ON sa.assigned_staff_id = s.staff_id
        LEFT OUTER JOIN Role r ON s.role_id = r.role_id
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
                    StaffId = reader.IsDBNull(reader.GetOrdinal("staff_id"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("staff_id")),

                    ShiftDate = reader.GetDateTime(reader.GetOrdinal("shift_date")),

                    ShiftType = reader.IsDBNull(reader.GetOrdinal("shift_type"))
                        ? "Unknown"
                        : reader.GetString(reader.GetOrdinal("shift_type")),

                    DepartmentId = reader.IsDBNull(reader.GetOrdinal("department_id"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("department_id")),

                    DepartmentName = reader.IsDBNull(reader.GetOrdinal("department_name"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("department_name")),

                    StaffName = reader.IsDBNull(reader.GetOrdinal("staff_name"))
                        ? "Unassigned"
                        : reader.GetString(reader.GetOrdinal("staff_name")),

                    RoleId = reader.IsDBNull(reader.GetOrdinal("role_id"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("role_id")),

                    ShiftStatus = reader.IsDBNull(reader.GetOrdinal("shift_status"))
                        ? "Unknown"
                        : reader.GetString(reader.GetOrdinal("shift_status"))
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
            d.department_id,
            d.name AS department_name,
                CASE 
                WHEN sa.assigned_staff_id IS NULL THEN 'Vacant'
                ELSE s.name
                END AS staff_name,
            r.role_name AS role,
            r.role_id AS role_id,
            sa.assigned_staff_id AS staff_id
        FROM PlannedShift sa
        INNER JOIN ShiftType st ON sa.shift_type_id = st.shift_type_id
        INNER JOIN Department d ON sa.department_id = d.department_id
        left outer JOIN Staff s ON sa.assigned_staff_id = s.staff_id
        LEFT OUTER JOIN Role r ON s.role_id = r.role_id
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
                    Role = reader.IsDBNull(reader.GetOrdinal("role"))
                        ? "Unassigned"
                        : reader.GetString(reader.GetOrdinal("role")),
                    DepartmentId = reader.IsDBNull(reader.GetOrdinal("department_id"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("department_id")),
                    RoleId = reader.IsDBNull(reader.GetOrdinal("role_id"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("role_id")),
                    StaffId = reader.IsDBNull(reader.GetOrdinal("staff_id"))
                            ? null
                            : reader.GetInt32(reader.GetOrdinal("staff_id")),
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



        ///// <summary>
        ///// Retrieves all upcoming shifts that have no staff assigned.
        ///// </summary>
        ///// <returns>List of uncovered shifts.</returns>
        //public async Task<List<PlannedShift>> GetUncoveredShiftsAsync()
        //{
        //    var uncoveredShifts = new List<PlannedShift>();

        //    using var conn = new SqlConnection(sqlConnectionString);
        //    await conn.OpenAsync();

        //    var query = @"
        //SELECT 
        //    ps.planned_shift_id,
        //    ps.shift_date,
        //    st.name AS shift_type_name,
        //    d.name AS department_name,
        //    ps.slot_number,
        //    ps.shift_status_id,
        //    ps.assigned_staff_id
        //FROM PlannedShift ps
        //JOIN ShiftType st ON ps.shift_type_id = st.shift_type_id
        //JOIN Department d ON ps.department_id = d.department_id
        //WHERE ps.assigned_staff_id IS NULL
        //AND ps.shift_date >= CAST(GETDATE() AS DATE)";

        //    using var cmd = new SqlCommand(query, conn);
        //    using var reader = await cmd.ExecuteReaderAsync();

        //    while (await reader.ReadAsync())
        //    {
        //        uncoveredShifts.Add(new PlannedShift
        //        {
        //            PlannedShiftId = reader.GetInt32(reader.GetOrdinal("planned_shift_id")),
        //            ShiftDate = reader.GetDateTime(reader.GetOrdinal("shift_date")),
        //            ShiftTypeName = reader.GetString(reader.GetOrdinal("shift_type_name")),
        //            DepartmentName = reader.GetString(reader.GetOrdinal("department_name")),
        //            SlotNumber = reader.GetInt32(reader.GetOrdinal("slot_number")),
        //            ShiftStatusId = reader.GetInt32(reader.GetOrdinal("shift_status_id")),
        //            AssignedStaffId = reader.IsDBNull(reader.GetOrdinal("assigned_staff_id"))
        //                ? null
        //                : reader.GetInt32(reader.GetOrdinal("assigned_staff_id"))
        //        });
        //    }

        //    return uncoveredShifts;
        //}


        /// <summary>
        /// Retrieves the shift type ID from the database by its name.
        /// </summary>
        /// <param name="shiftTypeName">Name of the shift type (e.g., "Morning", "Evening").</param>
        /// <returns>
        /// The ID of the shift type if found; otherwise, <c>null</c>.
        /// </returns>
        public async Task<int?> GetShiftTypeIdByNameAsync(string? shiftTypeName)
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


        public async Task<List<PlannedShift>> GetUncoveredShiftsAsync(GetUncoveredShiftsRequest? request = null)
        {
            var uncoveredShifts = new List<PlannedShift>();

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var query = @"
        SELECT 
            ps.planned_shift_id,
            ps.shift_date,
            st.name AS shift_type_name,
            d.name AS department_name,
            ps.slot_number,
            ps.shift_status_id,
            ps.assigned_staff_id
        FROM PlannedShift ps
        JOIN ShiftType st ON ps.shift_type_id = st.shift_type_id
        JOIN Department d ON ps.department_id = d.department_id
        LEFT JOIN Staff s ON ps.assigned_staff_id = s.staff_id
        WHERE ps.assigned_staff_id IS NULL";

            var cmd = new SqlCommand();
            cmd.Connection = conn;

            // Default to today's date if request is null or FromDate not set
            var fromDate = request?.FromDate.Date ?? DateTime.Today;
            query += " AND ps.shift_date >= @fromDate";
            cmd.Parameters.AddWithValue("@fromDate", fromDate);

            if (request != null)
            {
                if (request.ToDate.HasValue)
                {
                    query += " AND ps.shift_date <= @toDate";
                    cmd.Parameters.AddWithValue("@toDate", request.ToDate.Value.Date);
                }

                if (request.DepartmentId.HasValue)
                {
                    query += " AND ps.department_id = @departmentId";
                    cmd.Parameters.AddWithValue("@departmentId", request.DepartmentId.Value);
                }

                if (!string.IsNullOrEmpty(request.Role))
                {
                    query += " AND s.role = @role";
                    cmd.Parameters.AddWithValue("@role", request.Role);
                }

                if (!string.IsNullOrEmpty(request.ShiftType))
                {
                    query += " AND st.name = @shiftType";
                    cmd.Parameters.AddWithValue("@shiftType", request.ShiftType);
                }
            }

            cmd.CommandText = query;

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                uncoveredShifts.Add(new PlannedShift
                {
                    PlannedShiftId = reader.GetInt32(reader.GetOrdinal("planned_shift_id")),
                    ShiftDate = reader.GetDateTime(reader.GetOrdinal("shift_date")),
                    ShiftTypeName = reader.GetString(reader.GetOrdinal("shift_type_name")),
                    DepartmentName = reader.GetString(reader.GetOrdinal("department_name")),
                    SlotNumber = reader.GetInt32(reader.GetOrdinal("slot_number")),
                    ShiftStatusId = reader.GetInt32(reader.GetOrdinal("shift_status_id")),
                    AssignedStaffId = reader.IsDBNull(reader.GetOrdinal("assigned_staff_id"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("assigned_staff_id"))
                });
            }

            return uncoveredShifts;
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



    }
}
