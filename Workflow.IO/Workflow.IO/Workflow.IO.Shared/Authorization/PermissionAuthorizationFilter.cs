using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Workflow.IO.Shared.Authorization
{
    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly string _permission;
        private readonly IPermissionEngine _permissionEngine;
        private readonly IRoleProvider _roleProvider;

        public PermissionAuthorizationFilter(
            string permission,
            IPermissionEngine permissionEngine,
            IRoleProvider roleProvider)
        {
            _permission = permission;
            _permissionEngine = permissionEngine;
            _roleProvider = roleProvider;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var (orgRole, wsRole) = await _roleProvider.GetRolesAsync();

            if (!_permissionEngine.HasPermission(_permission, orgRole, wsRole))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
