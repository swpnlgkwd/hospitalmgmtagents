using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using HospitalStaffMgmtApis.Data.Models.Agent;
using HospitalStaffMgmtApis.Data.Repository.Interfaces;

namespace HospitalStaffMgmtApis.Data.Repository
{
    public class AgentConversationRepository : IAgentConversationRepository
    {
        private readonly string sqlConnectionString;

        public AgentConversationRepository(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        public async Task<AgentConversation> AddThreadForUser(AgentConversation agentConversation)
        {
            const string query = @"
MERGE AgentConversations AS target
USING (SELECT @UserId AS user_id, @ThreadId AS thread_id) AS source
ON target.user_id = source.user_id
WHEN MATCHED THEN
    UPDATE SET thread_id = source.thread_id, created_at = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (user_id, thread_id, created_at)
    VALUES (source.user_id, source.thread_id, GETDATE());";

            using var conn = new SqlConnection(sqlConnectionString);
            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@UserId", agentConversation.UserId);
            cmd.Parameters.AddWithValue("@ThreadId", agentConversation.ThreadId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            // Fetch it back to return the created object with created_at
            return await FetchThreadForUser(agentConversation.UserId);
        }

        public async Task DeleteThreadForUser(string threadId)
        {
            const string query = "DELETE FROM AgentConversations WHERE thread_id = @ThreadId";

            using var conn = new SqlConnection(sqlConnectionString);
            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@ThreadId", threadId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }


        public async Task<AgentConversation> FetchThreadForUser(string userId)
        {
            const string query = "SELECT user_id, thread_id, created_at FROM AgentConversations WHERE user_id = @UserId";

            using var conn = new SqlConnection(sqlConnectionString);
            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@UserId", userId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new AgentConversation
                {
                    UserId = reader.GetString(0),
                    ThreadId = reader.GetString(1),
                    CreatedAt = reader.GetDateTime(2)
                };
            }

            return null!;
        }

    }
}
