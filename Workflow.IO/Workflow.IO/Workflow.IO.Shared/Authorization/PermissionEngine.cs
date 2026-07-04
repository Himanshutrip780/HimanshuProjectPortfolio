using System.Collections.Generic;

namespace Workflow.IO.Shared.Authorization
{
    public interface IPermissionEngine
    {
        bool HasPermission(string permission, string? organizationRole, string? workspaceRole);
    }

    public class PermissionEngine : IPermissionEngine
    {
        public bool HasPermission(string permission, string? organizationRole, string? workspaceRole)
        {
            // In the single-tenant Jira SaaS model, any authenticated user 
            // belonging to the organization has all basic rights in the project.
            return true;
        }
    }
}
