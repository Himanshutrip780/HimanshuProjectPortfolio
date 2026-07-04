using System.Net;
using System.Net.Http.Headers;

namespace RealtimeApi.Clients
{
    public class TaskAccessClient : ITaskAccessClient
    {
        private readonly HttpClient _httpClient;

        public TaskAccessClient(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> CanAccessTaskAsync(
            Guid taskId,
            string? accessToken,
            string? orgId,
            CancellationToken cancellationToken = default)
        {
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/tasks/{taskId}");

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

            if (response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();

            return true;
        }
    }
}
