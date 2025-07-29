namespace HospitalSchedulingApp.Services.Interfaces
{
    /// <summary>
    /// Interface for generating JSON Web Tokens (JWT) for authenticated users.
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generates a JWT token for the specified staff member.
        /// </summary>
        /// <param name="staffId">The unique identifier of the staff member.</param>
        /// <param name="name">The full name of the staff member.</param>
        /// <param name="roleName">The role of the staff member (e.g., Scheduler, Employee).</param>
        /// <returns>A JWT token string containing user claims.</returns>
        string GenerateToken(int staffId, string name, string roleName);
    }
}
