using BCrypt.Net;
using HospitalSchedulingApp.Agent.Services;
using HospitalSchedulingApp.Dal.Entities;
using HospitalSchedulingApp.Dal.Repositories;
using HospitalSchedulingApp.Dtos.Auth;
using HospitalSchedulingApp.Services.Interfaces;

namespace HospitalSchedulingApp.Services
{
    /// <summary>
    /// Handles user authentication logic including login, logout, and validation.
    /// Manages thread mapping in the AI agent conversation system and token generation.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IRepository<UserCredential> _userCredentialRepo;
        private readonly IRepository<AgentConversations> _agentConversations;
        private readonly IRepository<Staff> _staffRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IAgentService _agentService;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            IRepository<UserCredential> userCredential,
            IRepository<Staff> staffRepo,
            IRepository<Role> roleRepo,
            IAgentService agentService,
            IRepository<AgentConversations> agentConversations,
            IJwtTokenService jwtTokenService)
        {
            _userCredentialRepo = userCredential;
            _staffRepo = staffRepo;
            _roleRepo = roleRepo;
            _agentService = agentService;
            _agentConversations = agentConversations;
            _jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Logs in a user by validating credentials and generating a JWT token.
        /// Associates the user with an agent thread if not already linked.
        /// </summary>
        /// <param name="loginRequestDto">User login credentials.</param>
        /// <returns>Login response with token and staff details.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when login fails.</exception>
        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var loginInfo = await ValidateLoginAsync(loginRequestDto);
            if (loginInfo == null)
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            var conversations = await _agentConversations.GetAllAsync();

            string threadId = "";
            var existingThread = conversations
                .FirstOrDefault(x => x.UserId == loginInfo.StaffId.ToString());

            

            if (existingThread?.ThreadId != null)
            {
                threadId = existingThread.ThreadId ?? "";
                loginInfo.ThreadId = threadId;
            }
            else
            {
                var thread = _agentService.CreateThread(); // Consider making this async

                threadId = thread.Id;
                await _agentConversations.AddAsync(new AgentConversations
                {
                    UserId = loginInfo.StaffId.ToString(),
                    ThreadId = threadId,
                    CreatedAt = DateTime.UtcNow
                });

                await _agentConversations.SaveAsync();
            
            }
            if(threadId != null)
                loginInfo.ThreadId = threadId;

            return loginInfo;
        }

        /// <summary>
        /// Validates user login credentials against stored records.
        /// </summary>
        /// <param name="loginRequest">Login credentials (username and password).</param>
        /// <returns>Login response if successful, otherwise null.</returns>
        public async Task<LoginResponseDto?> ValidateLoginAsync(LoginRequestDto loginRequest)
        {
            if (string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
                return null;

            var allUserCredentials = await _userCredentialRepo.GetAllAsync();
            var allStaff = await _staffRepo.GetAllAsync();
            var allRoles = await _roleRepo.GetAllAsync();

            var result = allUserCredentials
                .Where(uc => uc.Username.Equals(loginRequest.Username.Trim(), StringComparison.OrdinalIgnoreCase))
                .Join(allStaff,
                      uc => uc.StaffId,
                      s => s.StaffId,
                      (uc, s) => new { uc, s })
                .Where(joined => joined.s.IsActive)
                .Join(allRoles,
                      combined => combined.s.RoleId,
                      r => r.RoleId,
                      (combined, r) => new
                      {
                          combined.uc.PasswordHash,
                          combined.s.StaffId,
                          combined.s.StaffName,
                          RoleName = r.RoleName
                      })
                .FirstOrDefault();

            if (result == null)
                return null;

            if (string.IsNullOrWhiteSpace(result.PasswordHash) ||
                !BCrypt.Net.BCrypt.Verify(loginRequest.Password, result.PasswordHash))
                return null;

            return new LoginResponseDto
            {
                StaffId = result.StaffId,
                Name = result.StaffName,
                Role = result.RoleName,
                Token = _jwtTokenService.GenerateToken(result.StaffId, result.StaffName, result.RoleName)
            };
        }

        /// <summary>
        /// Logs out the user by removing the associated AI agent conversation thread.
        /// </summary>
        /// <param name="threadId">The thread ID to remove.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no thread is found.</exception>
        public async Task Logout(string threadId)
        {
            var allConversations = await _agentConversations.GetAllAsync();
            var agentConv = allConversations.FirstOrDefault(x => x.ThreadId == threadId);

            if (agentConv != null)
            {
                await _agentService.DeleteThreadForUser(threadId);
                _agentConversations.Delete(agentConv);
                await _agentConversations.SaveAsync();
            }
            else
            {
                throw new InvalidOperationException("No agent conversation found for the given thread ID.");
            }
        }
    }
}
