using System.Threading.Tasks;

namespace Workflow.IO.Shared.Authorization
{
    public interface IRoleProvider
    {
        Task<(string? organizationRole, string? workspaceRole)> GetRolesAsync();
    }
}
