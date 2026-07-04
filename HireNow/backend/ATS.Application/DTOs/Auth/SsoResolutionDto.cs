using System;

namespace ATS.Application.DTOs.Auth
{
    public class SsoResolutionDto
    {
        public bool SsoEnabled { get; set; }
        public string SsoProvider { get; set; }
        public string RedirectUrl { get; set; }
    }
}
