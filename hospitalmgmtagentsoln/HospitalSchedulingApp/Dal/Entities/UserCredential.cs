namespace HospitalSchedulingApp.Dal.Entities
{
    public class UserCredential
    {
        public int CredentialId { get; set; }
        public int StaffId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
