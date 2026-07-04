using JwtAuthenticationManager.Data;
using JwtAuthenticationManager.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workflow.IO.Shared.Contracts;
using Workflow.IO.Shared.Authorization;
using BCrypt.Net;

namespace UserApi.Controllers
{
    [ApiController]
    [Route("api/users/[controller]")]
    public class ApiKeysController : ControllerBase
    {
        private readonly AuthDbContext _authDb;
        private readonly ITenantContext _tenantContext;

        public ApiKeysController(AuthDbContext authDb, ITenantContext tenantContext)
        {
            _authDb = authDb;
            _tenantContext = tenantContext;
        }

        [HttpPost]
        [Authorize]
        [RequirePermission("billing.manage")] // High privilege required
        public async Task<IActionResult> GenerateKey([FromBody] GenerateApiKeyRequest request)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            if (!_tenantContext.CurrentOrganizationId.HasValue)
            {
                return BadRequest(new { success = false, message = "Organization context required" });
            }

            var rawKey = "zt_live_" + Guid.NewGuid().ToString("N");
            var keyHash = BCrypt.Net.BCrypt.HashPassword(rawKey);

            var apiKey = new ApiKey
            {
                Id = Guid.NewGuid(),
                KeyHash = keyHash,
                Prefix = rawKey.Substring(0, 16),
                Name = request.Name,
                OrganizationId = _tenantContext.CurrentOrganizationId.Value,
                WorkspaceId = _tenantContext.CurrentWorkspaceId.HasValue ? _tenantContext.CurrentWorkspaceId.Value : Guid.Empty,
                UserAccountId = userId,
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _authDb.ApiKeys.Add(apiKey);
            await _authDb.SaveChangesAsync();

            // ONLY return the rawKey once!
            return Ok(new
            {
                success = true,
                data = new
                {
                    id = apiKey.Id,
                    name = apiKey.Name,
                    rawKey = rawKey,
                    createdAt = apiKey.CreatedAt
                }
            });
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateKey([FromBody] ValidateApiKeyRequest request)
        {
            if (string.IsNullOrEmpty(request.ApiKey) || !request.ApiKey.StartsWith("zt_live_"))
            {
                return Unauthorized(new { success = false });
            }

            var prefix = request.ApiKey.Substring(0, 16);

            // Fetch potential keys by prefix
            var potentialKeys = await _authDb.ApiKeys
                .Where(k => k.Prefix == prefix && !k.IsRevoked)
                .ToListAsync();

            foreach (var key in potentialKeys)
            {
                if (BCrypt.Net.BCrypt.Verify(request.ApiKey, key.KeyHash))
                {
                    key.LastUsedAt = DateTime.UtcNow;
                    await _authDb.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            organizationId = key.OrganizationId,
                            workspaceId = key.WorkspaceId == Guid.Empty ? null : key.WorkspaceId.ToString(),
                            userId = key.UserAccountId
                        }
                    });
                }
            }

            return Unauthorized(new { success = false });
        }

        [HttpGet]
        [Authorize]
        [RequirePermission("billing.manage")]
        public async Task<IActionResult> ListKeys()
        {
            if (!_tenantContext.CurrentOrganizationId.HasValue)
            {
                return BadRequest(new { success = false });
            }

            var orgId = _tenantContext.CurrentOrganizationId.Value;

            var keys = await _authDb.ApiKeys
                .Where(k => k.OrganizationId == orgId && !k.IsRevoked)
                .Select(k => new
                {
                    id = k.Id,
                    name = k.Name,
                    prefix = k.Prefix,
                    createdAt = k.CreatedAt,
                    lastUsedAt = k.LastUsedAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = keys });
        }

        [HttpDelete("{id}")]
        [Authorize]
        [RequirePermission("billing.manage")]
        public async Task<IActionResult> RevokeKey(Guid id)
        {
            var key = await _authDb.ApiKeys.FindAsync(id);
            if (key == null) return NotFound();

            if (key.OrganizationId != _tenantContext.CurrentOrganizationId)
            {
                return Forbid();
            }

            key.IsRevoked = true;
            await _authDb.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }

    public class GenerateApiKeyRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class ValidateApiKeyRequest
    {
        public string ApiKey { get; set; } = string.Empty;
    }
}
