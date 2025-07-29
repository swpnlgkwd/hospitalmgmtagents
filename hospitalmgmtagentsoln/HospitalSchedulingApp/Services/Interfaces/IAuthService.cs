using HospitalSchedulingApp.Dtos.Auth;

namespace HospitalSchedulingApp.Services.Interfaces
{
    /// <summary>
    /// Provides authentication-related operations such as login and logout.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates the user and returns a JWT token if credentials are valid.
        /// </summary>
        /// <param name="loginRequest">Login credentials including username and password.</param>
        /// <returns>
        /// A <see cref="LoginResponseDto"/> containing authentication details if successful; otherwise, null.
        /// </returns>
        Task<LoginResponseDto?> Login(LoginRequestDto loginRequest);

        /// <summary>
        /// Logs out the user by removing the associated AI agent conversation thread.
        /// </summary>
        /// <param name="threadId">The thread ID to remove.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no thread is found.</exception>
        Task Logout(string threadId);
    }
}
