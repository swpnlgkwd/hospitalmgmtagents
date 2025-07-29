namespace HospitalSchedulingApp.Dtos.Auth
{
    /// <summary>
    /// Represents the response returned after a successful login,
    /// including authentication token and user details.
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// JWT token issued to the authenticated user.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Role name assigned to the user.
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Unique identifier for the staff member.
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// Full name of the authenticated user.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Associated thread ID for AI agent conversation context.
        /// </summary>
        public string ThreadId { get; set; } = string.Empty;
    }
}
