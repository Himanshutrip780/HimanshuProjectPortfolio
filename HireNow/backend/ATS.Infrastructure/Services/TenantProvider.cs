using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ATS.Application.Common.Interfaces;

namespace ATS.Infrastructure.Services
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetCompanyId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            // 1. Extract from JWT claims
            var claimValue = httpContext.User?.FindFirstValue("CompanyId");
            if (!string.IsNullOrEmpty(claimValue) && Guid.TryParse(claimValue, out var companyIdFromClaim))
            {
                return companyIdFromClaim;
            }

            // 2. Extract from header (for public requests)
            if (httpContext.Request.Headers.TryGetValue("X-Company-Id", out var headerValues))
            {
                var firstHeader = headerValues.ToString();
                if (Guid.TryParse(firstHeader, out var companyIdFromHeader))
                {
                    return companyIdFromHeader;
                }
            }

            // 3. Extract from query string (for public feeds/etc)
            if (httpContext.Request.Query.TryGetValue("companyId", out var queryValues))
            {
                var firstQuery = queryValues.ToString();
                if (Guid.TryParse(firstQuery, out var companyIdFromQuery))
                {
                    return companyIdFromQuery;
                }
            }

            // 4. Extract from route parameter
            if (httpContext.Request.RouteValues.TryGetValue("companyId", out var routeValue))
            {
                if (routeValue != null && Guid.TryParse(routeValue.ToString(), out var companyIdFromRoute))
                {
                    return companyIdFromRoute;
                }
            }

            return null;
        }
    }
}
