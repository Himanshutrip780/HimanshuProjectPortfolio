using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ATS.Application.Common.Interfaces;

namespace ATS.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";

        public string Role => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role) ?? "Candidate";

        public Guid? CompanyId
        {
            get
            {
                var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirstValue("CompanyId");
                if (Guid.TryParse(claimValue, out var companyId))
                {
                    return companyId;
                }
                return null;
            }
        }
    }
}
