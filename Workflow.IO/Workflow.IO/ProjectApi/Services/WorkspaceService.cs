using Microsoft.EntityFrameworkCore;
using ProjectApi.Data;
using ProjectApi.Model.Domain.Entities;
using ProjectApi.Model.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Workflow.IO.Shared.Contracts;
using Workflow.IO.Shared.Exceptions;

namespace ProjectApi.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly ProjectDbContext _dbContext;
        private readonly ITenantContext _tenantContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkspaceService(ProjectDbContext dbContext, ITenantContext tenantContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _tenantContext = tenantContext;
            _httpContextAccessor = httpContextAccessor;
        }

        private Guid GetUserId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdStr, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User context is missing");
        }

        public async Task<IEnumerable<Workspace>> GetWorkspacesAsync()
        {
            if (!_tenantContext.CurrentOrganizationId.HasValue)
            {
                throw new InvalidOperationException("Organization context is missing");
            }

            return await _dbContext.Workspaces
                .Where(w => w.OrganizationId == _tenantContext.CurrentOrganizationId.Value)
                .ToListAsync();
        }

        public async Task<Workspace?> CreateWorkspaceAsync(string name, string? description)
        {
            if (!_tenantContext.CurrentOrganizationId.HasValue)
            {
                throw new InvalidOperationException("Organization context is missing");
            }

            var userId = GetUserId();

            var workspace = new Workspace(name, description, _tenantContext.CurrentOrganizationId.Value);
            _dbContext.Workspaces.Add(workspace);

            await _dbContext.SaveChangesAsync();

            return workspace;
        }
    }
}
