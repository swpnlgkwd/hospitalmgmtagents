using HospitalStaffMgmtApis.Data.Repository.Interfaces;
using System.Data.SqlClient;

namespace HospitalStaffMgmtApis.Data.Repository
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly string sqlConnectionString;
        /// <summary>
        /// Initializes a new instance of the <see cref="StaffRepository"/> class.
        /// </summary>
        /// <param name="sqlConnectionString">The SQL connection string.</param>
        public DepartmentRepository(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
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
        /// Retrieves the department ID from the database by its name.
        /// </summary>
        /// <param name="departmentName">The exact name of the department.</param>
        /// <returns>
        /// The ID of the department if found; otherwise, <c>null</c>.
        /// </returns>
        public async Task<int?> GetDepartmentIdByNameAsync(string departmentName)
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

    }
}
