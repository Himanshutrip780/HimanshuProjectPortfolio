using System;

namespace ATS.Application.DTOs.Auth
{
    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
    }
}
