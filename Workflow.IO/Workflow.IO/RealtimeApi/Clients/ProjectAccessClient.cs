using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RealtimeApi.Clients
{
    public class ProjectAccessClient : IProjectAccessClient
    {
        private readonly HttpClient _httpClient;

        private readonly ILogger<ProjectAccessClient> _logger;

        public ProjectAccessClient(
            HttpClient httpClient,
            ILogger<ProjectAccessClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> IsProjectMemberAsync(
            Guid projectId,
            Guid userId,
            string? accessToken,
            string? orgId,
            CancellationToken cancellationToken = default)
        {
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/projects/{projectId}/members/me");

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        accessToken);
            }

            if (!string.IsNullOrWhiteSpace(orgId))
            {
                request.Headers.Add("X-Organization-ID", orgId);
            }

            using var response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            return response.IsSuccessStatusCode;
        }
    }
}
