using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealtimeApi.Clients;
using Workflow.IO.Shared.Exceptions;

namespace RealtimeApi.Hubs
{
    [Authorize]
    public class WorkflowIOHub : Hub
    {
        private readonly IProjectAccessClient _projectAccessClient;

        private readonly ITaskAccessClient _taskAccessClient;

        public WorkflowIOHub(
            IProjectAccessClient projectAccessClient,
            ITaskAccessClient taskAccessClient)
        {
            _projectAccessClient = projectAccessClient;
            _taskAccessClient = taskAccessClient;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();

            if (userId.HasValue)
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    UserGroup(userId.Value));
            }

            await base.OnConnectedAsync();
        }

        public async Task JoinProject(Guid projectId, string? orgId)
        {
            var userId =
                GetCurrentUserId() ??
                throw new UnauthorizedAccessException();

            var hasAccess =
                await _projectAccessClient.IsProjectMemberAsync(
                    projectId,
                    userId,
                    GetAccessToken(),
                    orgId);

            if (!hasAccess)
            {
                throw new ForbiddenException(
                    "User cannot join realtime updates for this project");
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                ProjectGroup(projectId));
        }

        public async Task LeaveProject(Guid projectId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                ProjectGroup(projectId));
        }

        public async Task JoinTask(Guid taskId, string? orgId)
        {
            var hasAccess =
                await _taskAccessClient.CanAccessTaskAsync(
                    taskId,
                    GetAccessToken(),
                    orgId);

            if (!hasAccess)
            {
                throw new ForbiddenException(
                    "User cannot join realtime updates for this task");
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                TaskGroup(taskId));
        }

        public async Task LeaveTask(Guid taskId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                TaskGroup(taskId));
        }

        public static string ProjectGroup(Guid projectId) =>
            $"project:{projectId}";

        public static string TaskGroup(Guid taskId) =>
            $"task:{taskId}";

        public static string UserGroup(Guid userId) =>
            $"user:{userId}";

        private Guid? GetCurrentUserId()
        {
            var userIdClaim =
                Context.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

            return Guid.TryParse(userIdClaim, out var userId)
                ? userId
                : null;
        }

        private string? GetAccessToken()
        {
            var httpContext =
                Context.GetHttpContext();

            var authorizationHeader =
                httpContext?
                    .Request
                    .Headers
                    .Authorization
                    .ToString();

            if (!string.IsNullOrWhiteSpace(authorizationHeader) &&
                authorizationHeader.StartsWith(
                    "Bearer ",
                    StringComparison.OrdinalIgnoreCase))
            {
                return authorizationHeader["Bearer ".Length..];
            }

            return httpContext?
                .Request
                .Query["access_token"]
                .ToString();
        }
    }
}
