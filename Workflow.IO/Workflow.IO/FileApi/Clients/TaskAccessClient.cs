using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace FileApi.Clients
{
    public class TaskAccessClient : ITaskAccessClient
    {
        private readonly HttpClient _httpClient;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public TaskAccessClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;

            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TaskAccessDto?> GetAccessibleTaskAsync(
            Guid taskId,
            CancellationToken cancellationToken = default)
        {
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/tasks/{taskId}");

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

            if (response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            using var document = JsonDocument.Parse(json);

            var data =
                document.RootElement.TryGetProperty(
                    "data",
                    out var dataElement)
                    ? dataElement
                    : document.RootElement;

            return data.Deserialize<TaskAccessDto>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
