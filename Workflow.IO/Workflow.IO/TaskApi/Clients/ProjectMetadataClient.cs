using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace TaskApi.Clients
{
    public class ProjectMetadataClient : IProjectMetadataClient
    {
        private readonly HttpClient _httpClient;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectMetadataClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;

            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProjectMetadataDto?> GetProjectMetadataAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/Project/{projectId}");

            ApplyAuthorizationHeader(request);

            using var response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            if (response.StatusCode is HttpStatusCode.NotFound
                or HttpStatusCode.Forbidden
                or HttpStatusCode.Unauthorized)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            using var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty(
                    "data",
                    out var data))
            {
                return null;
            }

            return new ProjectMetadataDto
            {
                ProjectId = data.GetProperty("projectId").GetGuid(),
                Key = data.GetProperty("key").GetString() ?? string.Empty
            };
        }

        private void ApplyAuthorizationHeader(HttpRequestMessage request)
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
        }
    }
}
