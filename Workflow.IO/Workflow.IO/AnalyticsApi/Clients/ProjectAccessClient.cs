using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AnalyticsApi.Clients
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
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/projects/{projectId}/members/me");

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

            using var response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            return response.IsSuccessStatusCode;
        }
    }
}
