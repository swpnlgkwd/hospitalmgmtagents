using HospitalStaffMgmtApis.Data.Model;
using HospitalStaffMgmtApis.Data.Models;
using HospitalStaffMgmtApis.Data.Models.StaffLeaveRequest;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace HospitalStaffMgmtApis.Data.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly string sqlConnectionString;
        /// <summary>
        /// Initializes a new instance of the <see cref="StaffRepository"/> class.
        /// </summary>
        /// <param name="sqlConnectionString">The SQL connection string.</param>
        public LeaveRequestRepository(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        // Add for Leave
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


        // view leave requests by staff id, department, and date range,by role

        // Approve or reject leave requests

        // cancel leave request

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
                //var availableStaff = await FindAvailableStaffForShiftReplacementAsync(impactedShift);

                //if (availableStaff.Any())
                //{
                //    // Currently using the first match (can be enhanced later)
                //    var selected = availableStaff.First();

                //    // Attempt to assign new staff to the impacted shift
                //    var assignResult = await AssignStaffToShiftAsync(new AutoAssignShiftRequest
                //    {
                //        StaffId = selected.StaffId,
                //        ShiftDate = impactedShift.ShiftDate.ToString("yyyy-MM-dd"),
                //        ShiftType = impactedShift.ShiftType,
                //        ShiftId = impactedShift.ShiftId
                //    });

                //    response.AssignedShifts.Add(new AutoReplaceShiftsForLeaveResult
                //    {
                //        ShiftDate = impactedShift.ShiftDate,
                //        ShiftType = impactedShift.ShiftType,
                //        Department = impactedShift.Department,
                //        Role = impactedShift.Role,
                //        AssignedTo = selected.Name,
                //        Success = assignResult == "Shift reassigned successfully.",
                //        Message = assignResult
                //    });
                //}
                //else
                //{
                //    // No staff available to replace the impacted shift
                //    response.UnassignedShifts.Add(new AutoReplaceShiftsForLeaveResult
                //    {
                //        ShiftDate = impactedShift.ShiftDate,
                //        ShiftType = impactedShift.ShiftType,
                //        Department = impactedShift.Department,
                //        Role = impactedShift.Role,
                //        Success = false,
                //        Message = "No available replacement found."
                //    });
                //}
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


        ///// <summary>
        ///// Retrieves all leave requests that are pending approval.
        ///// </summary>
        ///// <returns>List of pending leave requests.</returns>
        //public async Task<List<PendingLeaveResponse>> GetPendingLeaveRequestsAsync()
        //{
        //    var pendingRequests = new List<PendingLeaveResponse>();

        //    using var conn = new SqlConnection(sqlConnectionString);
        //    await conn.OpenAsync();

        //    var query = @"
        //SELECT 
        //    lr.id,
        //    s.name AS staff_name,
        //    d.name AS department_name,
        //    lr.leave_start,
        //    lr.leave_end
        //FROM LeaveRequests lr
        //INNER JOIN Staff s ON lr.staff_id = s.staff_id
        //INNER JOIN Department d ON s.department_id = d.department_id
        //WHERE lr.status = 'Pending'";

        //    using var cmd = new SqlCommand(query, conn);
        //    using var reader = await cmd.ExecuteReaderAsync();

        //    while (await reader.ReadAsync())
        //    {
        //        pendingRequests.Add(new PendingLeaveResponse
        //        {
        //            LeaveRequestId = reader.GetInt32(reader.GetOrdinal("id")),
        //            StaffName = reader.GetString(reader.GetOrdinal("staff_name")),
        //            LeaveStartDate = reader.GetDateTime(reader.GetOrdinal("leave_start")),
        //            LeaveEndDate = reader.GetDateTime(reader.GetOrdinal("leave_end")),
        //            DepartmentName = reader.GetString(reader.GetOrdinal("department_name"))
        //        });
        //    }

        //    return pendingRequests;
        //}


        /// <summary>
        /// Retrieves all leave requests that are pending approval.
        /// </summary>
        /// <returns>List of pending leave requests.</returns>
        /// <summary>
        /// Fetches all pending leave requests, optionally filtered by date range, staff name, or department name.
        /// </summary>
        /// <param name="pendingLeaveRequest">Filter criteria for retrieving pending requests.</param>
        /// <returns>List of matching leave requests.</returns>
        public async Task<List<PendingLeaveResponse>> FetchPendingLeaveRequestsAsync(PendingLeaveRequest? pendingLeaveRequest = null)
        {
            var pendingRequests = new List<PendingLeaveResponse>();

            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var query = @"
        SELECT 
            s.name AS staff_name,
            d.name AS department_name,
            lr.leave_start,
            lr.leave_end
        FROM LeaveRequests lr
        INNER JOIN Staff s ON lr.staff_id = s.staff_id
        INNER JOIN Department d ON s.department_id = d.department_id
        WHERE lr.status = 'Pending'";

            var filters = new List<string>();
            var cmd = new SqlCommand();
            cmd.Connection = conn;

            if (pendingLeaveRequest != null)
            {
                if (!string.IsNullOrEmpty(pendingLeaveRequest.FromDate))
                {
                    filters.Add("lr.leave_start >= @fromDate");
                    cmd.Parameters.AddWithValue("@fromDate", DateTime.Parse(pendingLeaveRequest.FromDate));
                }

                if (!string.IsNullOrEmpty(pendingLeaveRequest.ToDate))
                {
                    filters.Add("lr.leave_end <= @toDate");
                    cmd.Parameters.AddWithValue("@toDate", DateTime.Parse(pendingLeaveRequest.ToDate));
                }

                if (!string.IsNullOrEmpty(pendingLeaveRequest.StaffName))
                {
                    filters.Add("s.name LIKE @staffName");
                    cmd.Parameters.AddWithValue("@staffName", $"%{pendingLeaveRequest.StaffName}%");
                }

                if (!string.IsNullOrEmpty(pendingLeaveRequest.DepartmentName))
                {
                    filters.Add("d.name LIKE @departmentName");
                    cmd.Parameters.AddWithValue("@departmentName", $"%{pendingLeaveRequest.DepartmentName}%");
                }
            }

            if (filters.Count > 0)
            {
                query += " AND " + string.Join(" AND ", filters);
            }

            cmd.CommandText = query;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pendingRequests.Add(new PendingLeaveResponse
                {
                    StaffName = reader.GetString(reader.GetOrdinal("staff_name")),
                    DepartmentName = reader.GetString(reader.GetOrdinal("department_name")),
                    LeaveStartDate = reader.GetDateTime(reader.GetOrdinal("leave_start")),
                    LeaveEndDate = reader.GetDateTime(reader.GetOrdinal("leave_end"))
                });
            }

            return pendingRequests;
        }

    }
}
