namespace HospitalStaffMgmtApis.Data.Model.Auth
{
    public class LoginQueryResult
    {
        public int staff_id { get; set; }
        public string name { get; set; } = string.Empty;
        public string role_name { get; set; } = string.Empty;
        public string password_hash { get; set; } = string.Empty;
    }

}
