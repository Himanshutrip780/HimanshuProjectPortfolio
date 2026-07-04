using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ActivityApi.Clients
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

        public async Task<bool> IsTaskAccessibleAsync(
            Guid taskId,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/tasks/{taskId}");

            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authorizationHeader))
            {
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorizationHeader);
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

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
    }
}
