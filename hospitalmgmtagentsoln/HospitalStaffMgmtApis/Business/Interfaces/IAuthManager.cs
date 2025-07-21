using System.Threading.Tasks;
using HospitalStaffMgmtApis.Data.Model.Account;

namespace HospitalStaffMgmtApis.Business.Interfaces
{
    /// <summary>
    /// Interface for authentication-related business logic.
    /// </summary>
    public interface IAuthManager
    {
        /// <summary>
        /// Validates the login credentials and returns user details with a token if successful.
        /// </summary>
        /// <param name="request">The login request containing username and password.</param>
        /// <returns>
        /// A <see cref="LoginResponse"/> object if credentials are valid; otherwise, null.
        /// </returns>
        Task<LoginResponse?> ValidateLoginAsync(LoginRequest request);
    }
}
