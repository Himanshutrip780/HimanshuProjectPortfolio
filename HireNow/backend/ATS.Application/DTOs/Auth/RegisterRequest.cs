using System;

namespace ATS.Application.DTOs.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string? Domain { get; set; }
        public Guid? CompanyId { get; set; }
        public string? Role { get; set; }
    }
}
