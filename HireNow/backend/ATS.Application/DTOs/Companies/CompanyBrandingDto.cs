using System;

namespace ATS.Application.DTOs.Companies
{
    public class CompanyBrandingDto
    {
        public string CompanyName { get; set; }
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? FontFamily { get; set; }
        public string? CustomCss { get; set; }
    }
}
