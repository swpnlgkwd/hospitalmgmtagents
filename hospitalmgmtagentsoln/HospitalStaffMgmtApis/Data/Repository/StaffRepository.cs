using Azure.Core;
using HospitalStaffMgmtApis.Data.Model;
using HospitalStaffMgmtApis.Data.Model.HospitalStaffMgmtApis.Models.Requests.HospitalStaffMgmtApis.Models.Requests;
using HospitalStaffMgmtApis.Data.Models;
using HospitalStaffMgmtApis.Data.Models.Shift;
using HospitalStaffMgmtApis.Data.Models.Staff;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using HospitalStaffMgmtApis.Models;
using HospitalStaffMgmtApis.Models.Requests;
using Microsoft.Recognizers.Text.DateTime;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json.Serialization;

namespace HospitalStaffMgmtApis.Data.Repository
{

    // Implementation of staff repository using SQL Server
    public class StaffRepository : IStaffRepository
    {
        private readonly string sqlConnectionString;
        private readonly IShiftRepository shiftRepository;

        private readonly IDepartmentRepository departmentRepository;
        /// <summary>
        /// Initializes a new instance of the <see cref="StaffRepository"/> class.
        /// </summary>
        /// <param name="sqlConnectionString">The SQL connection string.</param>
        public StaffRepository(string sqlConnectionString,
            IShiftRepository shiftRepository,
            IDepartmentRepository departmentRepository)
        {
            this.sqlConnectionString = sqlConnectionString;
            this.shiftRepository = shiftRepository;
            this.departmentRepository = departmentRepository;
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
        /// Finds staff available for assignment during a given date or range, based on department, role, and shift type.
        /// </summary>
        /// <param name="request">Request parameters including date range, department, role, and shift type.</param>
        /// <returns>List of available staff who match criteria.</returns>
        public async Task<List<AvailableStaffResponse>> FindAvailableStaffAsync(AvailableStaffRequest request)
        {
            var result = new List<AvailableStaffResponse>();

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
                var shiftTypeId = await shiftRepository.GetShiftTypeIdByNameAsync(request.ShiftType);
                cmd.Parameters.AddWithValue("@shiftType", shiftTypeId);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new AvailableStaffResponse
                {
                    StaffId = reader.GetInt32(reader.GetOrdinal("staff_id")),
                    StaffName = reader.GetString(reader.GetOrdinal("staff_name")),
                    DepartmentName = reader.GetString(reader.GetOrdinal("department_name")),
                    RoleName = reader.GetString(reader.GetOrdinal("role_name")),
                    AvailableDate = request.FromDate
                });
            }

            return result;
        }

        /// <summary>
        /// Retrieves all leave requests that are pending approval.
        /// </summary>
        /// <returns>List of pending leave requests.</returns>
        public async Task<List<PlannedShift>> GetFatiguedStaffAsync(FatiqueStaffRequest? fatiqueStaffRequest = null)
        {
            var plannedShifts = new List<PlannedShift>();

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var filters = new List<string>();
            var parameters = new List<SqlParameter>();

            if (fatiqueStaffRequest != null)
            {
                if (fatiqueStaffRequest.StartDate != default)
                {
                    filters.Add("n1.shift_date >= @StartDate");
                    parameters.Add(new SqlParameter("@StartDate", fatiqueStaffRequest.StartDate));
                }

                if (fatiqueStaffRequest.EndDate != default)
                {
                    filters.Add("n1.shift_date <= @EndDate");
                    parameters.Add(new SqlParameter("@EndDate", fatiqueStaffRequest.EndDate));
                }

                if (!string.IsNullOrWhiteSpace(fatiqueStaffRequest.StaffName))
                {
                    filters.Add("n1.staff_name LIKE @StaffName");
                    parameters.Add(new SqlParameter("@StaffName", $"%{fatiqueStaffRequest.StaffName}%"));
                }
            }

            var whereClause = filters.Count > 0 ? "WHERE " + string.Join(" AND ", filters) : "";

            var query = $@"
WITH NightShifts AS (
    SELECT 
        ps.planned_shift_id,
        ps.assigned_staff_id,
        ps.shift_date,
        ps.department_id,
        ps.shift_type_id,
        ps.slot_number,
        ps.shift_status_id,
        d.name AS department_name,
        st.name AS shift_type_name,
        s.name AS staff_name
    FROM PlannedShift ps
    INNER JOIN ShiftType st ON ps.shift_type_id = st.shift_type_id
    INNER JOIN Department d ON ps.department_id = d.department_id
    INNER JOIN Staff s ON ps.assigned_staff_id = s.staff_id
    WHERE st.name = 'Night' AND ps.assigned_staff_id IS NOT NULL
),
ConsecutiveNights AS (
    SELECT 
        n1.*
    FROM NightShifts n1
    JOIN NightShifts n2 
        ON n1.assigned_staff_id = n2.assigned_staff_id
       AND DATEDIFF(DAY, n1.shift_date, n2.shift_date) = 1
)
SELECT 
    planned_shift_id,
    assigned_staff_id,
    shift_date,
    department_id,
    shift_type_id,
    slot_number,
    shift_status_id,
    department_name,
    shift_type_name,
    staff_name
FROM ConsecutiveNights n1
{whereClause}
ORDER BY n1.shift_date, n1.staff_name
";

            using var cmd = new SqlCommand(query, conn);
            foreach (var param in parameters)
            {
                cmd.Parameters.Add(param);
            }

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                plannedShifts.Add(new PlannedShift
                {
                    PlannedShiftId = reader.GetInt32(reader.GetOrdinal("planned_shift_id")),
                    AssignedStaffId = reader.GetInt32(reader.GetOrdinal("assigned_staff_id")),
                    ShiftDate = reader.GetDateTime(reader.GetOrdinal("shift_date")),
                    DepartmentName = reader.GetString(reader.GetOrdinal("department_name")),
                    ShiftTypeName = reader.GetString(reader.GetOrdinal("shift_type_name")),
                    StaffName = reader.GetString(reader.GetOrdinal("staff_name"))
                });
            }

            return plannedShifts;
        }

        /// <summary>
        /// Finds available staff who are not assigned, not on leave, and marked available for a given shift.
        /// </summary>
        /// <param name="request">Shift details impacted by leave.</param>
        /// <returns>List of available staff for replacement.</returns>
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

            var departmentId = await departmentRepository.GetDepartmentIdByNameAsync(request.Department);
            var shiftTypeID = await shiftRepository.GetShiftTypeIdByNameAsync(request.ShiftType.Trim());

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


        #region Helpers





        #endregion


    }
}

