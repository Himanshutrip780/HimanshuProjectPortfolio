using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectApi.Services;
using System.Threading.Tasks;
using Workflow.IO.Shared.Authorization;

namespace ProjectApi.Controllers
{
    [ApiController]
    [Route("api/project/workspaces")]
    [Authorize]
    [RequirePermission(Permissions.WorkspaceManage)]
    public class WorkspaceController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;

        public WorkspaceController(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkspaces()
        {
            var workspaces = await _workspaceService.GetWorkspacesAsync();
            return Ok(new { success = true, data = workspaces });
        }

        public class CreateWorkspaceRequest
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkspace([FromBody] CreateWorkspaceRequest request)
        {
            var workspace = await _workspaceService.CreateWorkspaceAsync(request.Name, request.Description);
            return Ok(new { success = true, data = workspace });
        }
    }
}
