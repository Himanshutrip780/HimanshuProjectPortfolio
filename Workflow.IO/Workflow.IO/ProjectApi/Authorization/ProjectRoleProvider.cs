using Microsoft.EntityFrameworkCore;
using ProjectApi.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Workflow.IO.Shared.Authorization;
using Workflow.IO.Shared.Contracts;

namespace ProjectApi.Authorization
{
    public class ProjectRoleProvider : IRoleProvider
    {
        private readonly ProjectDbContext _dbContext;
        private readonly ITenantContext _tenantContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectRoleProvider(ProjectDbContext dbContext, ITenantContext tenantContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _tenantContext = tenantContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(string? organizationRole, string? workspaceRole)> GetRolesAsync()
        {
            // In a real implementation, organization role would likely come from Gateway headers or claims
            // For now, we will extract the workspace role from our local database.
            
            string? orgRole = _httpContextAccessor.HttpContext?.Request.Headers["X-Organization-Role"].FirstOrDefault();
            string? wsRole = null;

            // Since user has all basic rights in single tenant model, workspace role is inherited from org role
            wsRole = orgRole;

            return (orgRole, wsRole);
        }
    }
}
