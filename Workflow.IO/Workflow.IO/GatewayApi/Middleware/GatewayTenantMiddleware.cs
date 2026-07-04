using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GatewayApi.Middleware
{
    public class GatewayTenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GatewayTenantMiddleware> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly string _userApiBaseUrl;

        public GatewayTenantMiddleware(
            RequestDelegate next, 
            ILogger<GatewayTenantMiddleware> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IMemoryCache cache)
        {
            _next = next;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            var configuredUrl = configuration["ReverseProxy:Clusters:users:Destinations:destination1:Address"];
            _userApiBaseUrl = !string.IsNullOrEmpty(configuredUrl) ? configuredUrl.TrimEnd('/') : "http://localhost:5240";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 0. API KEY AUTHENTICATION
            var apiKey = context.Request.Headers["x-api-key"].FirstOrDefault();
            if (!string.IsNullOrEmpty(apiKey))
            {
                var tenantInfo = await ValidateApiKeyAsync(apiKey);
                if (tenantInfo != null)
                {
                    // If API key is valid, inject tenant headers and skip subdomain/JWT logic
                    context.Request.Headers["X-Organization-ID"] = tenantInfo.OrganizationId;
                    if (!string.IsNullOrEmpty(tenantInfo.WorkspaceId))
                    {
                        context.Request.Headers["X-Workspace-ID"] = tenantInfo.WorkspaceId;
                    }
                    // Optionally inject a user ID header
                    context.Request.Headers["X-Internal-User-ID"] = tenantInfo.UserId;
                    
                    await _next(context);
                    return;
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Invalid API Key.");
                    return;
                }
            }

            // 1. Dynamic Subdomain Extraction
            var host = context.Request.Host.Host;
            var subdomain = ExtractSubdomain(host);
            string? organizationIdStr = null;

            if (!string.IsNullOrEmpty(subdomain) && subdomain != "www" && subdomain != "app" && subdomain != "localhost")
            {
                // Resolve Subdomain
                var orgId = await ResolveSubdomainAsync(subdomain);
                if (orgId == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.WriteAsync("Workspace not found.");
                    return;
                }
                organizationIdStr = orgId;
            }
            else
            {
                // Fallback to explicit header if not using subdomain routing
                organizationIdStr = context.Request.Headers["X-Organization-ID"].FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(organizationIdStr))
            {
                // Ignore frontend routing artifacts passed as orgId
                var reservedWords = new[] { "projects", "tasks", "calendar", "teams", "settings", "reports", "clients", "analytics", "profile", "admin", "notifications", "auth", "team", "users", "user", "web", "frontend", "gateway", "gatewayapi" };
                if (reservedWords.Contains(organizationIdStr.ToLowerInvariant()))
                {
                    organizationIdStr = null;
                }
            }

            if (!string.IsNullOrEmpty(organizationIdStr))
            {
                // Inject downstream header
                context.Request.Headers["X-Organization-ID"] = organizationIdStr;
            }

            var workspaceIdStr = context.Request.Headers["X-Workspace-ID"].FirstOrDefault();
            if (!string.IsNullOrEmpty(workspaceIdStr))
            {
                context.Request.Headers["X-Workspace-ID"] = workspaceIdStr;
                context.Request.Headers["Tenant-Id"] = workspaceIdStr;
            }

            // Public path bypass after injecting headers
            if (IsPublicPath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            if (!string.IsNullOrEmpty(organizationIdStr))
            {
                // Verify Organization Membership
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var isValidMember = await VerifyMembershipAsync(context, organizationIdStr);
                    if (!isValidMember)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        await context.Response.WriteAsync("Access to this workspace is forbidden.");
                        return;
                    }
                }
            }

            await _next(context);
        }

        private class ApiKeyValidationResult
        {
            public string OrganizationId { get; set; } = string.Empty;
            public string? WorkspaceId { get; set; }
            public string UserId { get; set; } = string.Empty;
        }

        private async Task<ApiKeyValidationResult?> ValidateApiKeyAsync(string apiKey)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient("TenantValidator");
                
                var payload = new { apiKey = apiKey };
                var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_userApiBaseUrl}/api/users/apikeys/validate", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseContent);
                    if (doc.RootElement.TryGetProperty("data", out var dataElement))
                    {
                        return new ApiKeyValidationResult
                        {
                            OrganizationId = dataElement.GetProperty("organizationId").GetString() ?? string.Empty,
                            WorkspaceId = dataElement.TryGetProperty("workspaceId", out var wId) && wId.ValueKind != JsonValueKind.Null ? wId.GetString() : null,
                            UserId = dataElement.GetProperty("userId").GetString() ?? string.Empty
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate API Key.");
            }
            return null;
        }

        private async Task<string?> ResolveSubdomainAsync(string subdomain)
        {
            var cacheKey = $"subdomain_orgid_{subdomain.ToLowerInvariant()}";
            if (_cache.TryGetValue<string>(cacheKey, out var cachedOrgId))
            {
                return cachedOrgId;
            }

            try
            {
                using var client = _httpClientFactory.CreateClient("TenantResolver");
                var response = await client.GetAsync($"{_userApiBaseUrl}/api/user/organizations/by-subdomain/{subdomain}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out var dataElement))
                    {
                        if (dataElement.TryGetProperty("organizationId", out var idElement))
                        {
                            var orgId = idElement.GetString();
                            if (!string.IsNullOrEmpty(orgId))
                            {
                                _cache.Set(cacheKey, orgId, TimeSpan.FromMinutes(15));
                                return orgId;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve subdomain {Subdomain}", subdomain);
            }
            return null;
        }

        private async Task<bool> VerifyMembershipAsync(HttpContext context, string organizationId)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient("TenantValidator");
                
                // Forward the auth header
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    client.DefaultRequestHeaders.Add("Authorization", authHeader);
                }

                var response = await client.GetAsync($"{_userApiBaseUrl}/api/user/me/organization");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("VerifyMembershipAsync UserApi returned 200. Content: {Content}", content);
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Object)
                    {
                        if (dataElement.TryGetProperty("organizationId", out var idElement))
                        {
                            var resolvedId = idElement.GetString();
                            var resolvedSubdomain = dataElement.TryGetProperty("subdomain", out var subElement) ? subElement.GetString() : null;

                            _logger.LogWarning("VerifyMembershipAsync resolving: expected={Expected}, actualId={ActualId}, actualSubdomain={ActualSubdomain}", organizationId, resolvedId, resolvedSubdomain);
                            if (string.Equals(resolvedId, organizationId, StringComparison.OrdinalIgnoreCase) || 
                                (!string.IsNullOrEmpty(resolvedSubdomain) && string.Equals(resolvedSubdomain, organizationId, StringComparison.OrdinalIgnoreCase)))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            _logger.LogWarning("VerifyMembershipAsync UserApi response data does not contain organizationId property!");
                        }
                    }
                }
                else 
                {
                     _logger.LogWarning("VerifyMembershipAsync UserApi returned non-success: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify membership for organization {OrgId}", organizationId);
            }
            
            return false;
        }

        private string? ExtractSubdomain(string host)
        {
            if (string.IsNullOrWhiteSpace(host)) return null;
            
            // Ignore if the host is an IP address (like 127.0.0.1)
            if (System.Net.IPAddress.TryParse(host, out _)) return null;

            var parts = host.Split('.');

            // Ignore Azure Container Apps default domains entirely so they rely on headers instead of subdomain
            if (host.EndsWith(".azurecontainerapps.io", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Ignore render.com default domains (e.g., myapp.onrender.com)
            if (host.EndsWith(".onrender.com", StringComparison.OrdinalIgnoreCase))
            {
                if (parts.Length > 3) return parts[0];
                return null;
            }

            if (parts.Length > 1 && parts[0] != "localhost")
            {
                return parts[0];
            }
            
            if (host.EndsWith(".localhost") && parts.Length > 1)
            {
                 return parts[0];
            }
            
            return null;
        }

        private bool IsPublicPath(PathString path)
        {
            var value = path.Value?.ToLowerInvariant() ?? string.Empty;
            return value == "/" ||
                   value.StartsWith("/health") ||
                   value.StartsWith("/ready") ||
                   value.Contains("/users/authenticate") ||
                   value.Contains("/users/register") ||
                   value.Contains("/users/refresh") ||
                   value.Contains("/user/authenticate") ||
                   value.Contains("/user/register") ||
                   value.Contains("/user/refresh") ||
                   value.Contains("/organizations/by-subdomain");
        }
    }
}
