﻿namespace HospitalStaffMgmtApis.Data.Model.Account
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int StaffId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
