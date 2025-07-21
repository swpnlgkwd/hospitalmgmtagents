using HospitalStaffMgmtApis.Data.Model.Auth;
using HospitalStaffMgmtApis.Data.Repository.Auth;
using System.Data.SqlClient;

namespace HospitalStaffMgmtApis.Data.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly string sqlConnectionString;

        public AuthRepository(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        // Fetch login details by username (used for password validation and token generation)
        public async Task<LoginQueryResult?> GetLoginInfoByUsernameAsync(string username)
        {
            using var conn = new SqlConnection(sqlConnectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    s.staff_id, s.name, r.role_name, uc.password_hash
                FROM Staff s
                JOIN Role r ON s.role_id = r.role_id
                JOIN UserCredential uc ON s.staff_id = uc.staff_id
                WHERE uc.username = @username AND s.is_active = 1";
            cmd.Parameters.AddWithValue("@username", username.Trim());

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new LoginQueryResult
                {
                    staff_id = reader.GetInt32(reader.GetOrdinal("staff_id")),
                    name = reader.GetString(reader.GetOrdinal("name")),
                    role_name = reader.GetString(reader.GetOrdinal("role_name")),
                    password_hash = reader.GetString(reader.GetOrdinal("password_hash"))
                };
            }

            return null;
        }
    }
}
