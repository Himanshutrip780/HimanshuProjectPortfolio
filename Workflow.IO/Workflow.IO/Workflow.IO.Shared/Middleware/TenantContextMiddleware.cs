using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Workflow.IO.Shared.Contracts;

namespace Workflow.IO.Shared.Middleware
{
    public class TenantContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantContextMiddleware> _logger;

        public TenantContextMiddleware(RequestDelegate next, ILogger<TenantContextMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
        {
            _logger.LogInformation("[TenantContextMiddleware] Path: {Path}, Method: {Method}", context.Request.Path, context.Request.Method);
            foreach (var header in context.Request.Headers)
            {
                if (header.Key.StartsWith("X-", StringComparison.OrdinalIgnoreCase) || header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("  Header: {Key} = {Value}", header.Key, header.Value);
                }
            }

            if (context.Request.Headers.TryGetValue("X-Organization-ID", out var orgIdValues))
            {
                var orgIdStr = orgIdValues.ToString();
                if (Guid.TryParse(orgIdStr, out var orgId))
                {
                    tenantContext.CurrentOrganizationId = orgId;
                }
                else
                {
                    _logger.LogWarning("  Failed to parse X-Organization-ID as Guid: '{OrgIdStr}'", orgIdStr);
                }
            }

            if (context.Request.Headers.TryGetValue("X-Workspace-ID", out var workspaceIdValues))
            {
                var workspaceIdStr = workspaceIdValues.ToString();
                if (Guid.TryParse(workspaceIdStr, out var workspaceId))
                {
                    tenantContext.CurrentWorkspaceId = workspaceId;
                }
                else
                {
                    _logger.LogWarning("  Failed to parse X-Workspace-ID as Guid: '{WorkspaceIdStr}'", workspaceIdStr);
                }
            }
            
            if (!tenantContext.CurrentWorkspaceId.HasValue && tenantContext.CurrentOrganizationId.HasValue)
            {
                tenantContext.CurrentWorkspaceId = tenantContext.CurrentOrganizationId.Value;
            }

            _logger.LogInformation("  TenantContext: OrgId = {OrgId}, WorkspaceId = {WorkspaceId}", tenantContext.CurrentOrganizationId, tenantContext.CurrentWorkspaceId);

            await _next(context);
        }
    }
}
