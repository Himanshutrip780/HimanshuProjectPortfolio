using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace TaskApi.Clients
{
    public class ProjectAccessClient : IProjectAccessClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProjectAccessClient> _logger;

        public ProjectAccessClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProjectAccessClient> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> IsProjectMemberAsync(
            Guid projectId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await HasProjectAccessAsync(projectId, cancellationToken);
        }

        public async Task<bool> CanContributeAsync(
            Guid projectId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            // In single-tenant Jira mode, any authenticated user with access to the project can contribute
            return await HasProjectAccessAsync(projectId, cancellationToken);
        }

        private async Task<bool> HasProjectAccessAsync(
            Guid projectId,
            CancellationToken cancellationToken)
        {
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/Project/{projectId}");

            ApplyHeaders(request);

            using var response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            return response.IsSuccessStatusCode;
        }

        private void ApplyHeaders(HttpRequestMessage request)
        {
            var authorizationHeader =
                _httpContextAccessor
                    .HttpContext?
                    .Request
                    .Headers
                    .Authorization
                    .ToString();

            if (!string.IsNullOrWhiteSpace(authorizationHeader))
            {
                request.Headers.Authorization =
                    AuthenticationHeaderValue.Parse(
                        authorizationHeader);
            }

            var orgIdHeader = _httpContextAccessor.HttpContext?.Request.Headers["X-Organization-ID"].ToString();
            var workspaceIdHeader = _httpContextAccessor.HttpContext?.Request.Headers["X-Workspace-ID"].ToString();

            if (!string.IsNullOrWhiteSpace(orgIdHeader))
            {
                request.Headers.Add("X-Organization-ID", orgIdHeader);
            }

            if (!string.IsNullOrWhiteSpace(workspaceIdHeader))
            {
                request.Headers.Add("X-Workspace-ID", workspaceIdHeader);
            }
        }
    }
}
