using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TaskApi.Services
{
    public class MicrosoftGraphEmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MicrosoftGraphEmailService> _logger;

        public MicrosoftGraphEmailService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<MicrosoftGraphEmailService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string[] toAddresses, string subject, string htmlBody)
        {
            _logger.LogInformation("Generating daily sprint email update with subject: {Subject}", subject);

            // Write HTML file to disk for preview and verification
            try
            {
                var backupsDir = Path.Combine(Directory.GetCurrentDirectory(), "backups");
                if (!Directory.Exists(backupsDir))
                {
                    Directory.CreateDirectory(backupsDir);
                }

                var previewPath = Path.Combine(backupsDir, "sprint-email-preview.html");
                await File.WriteAllTextAsync(previewPath, htmlBody, Encoding.UTF8);
                _logger.LogInformation("Email preview HTML successfully written to: {PreviewPath}", previewPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write email preview HTML file to backups directory.");
            }

            // Attempt to send email via Microsoft Graph API if configured
            var clientId = _configuration["MicrosoftGraph:ClientId"];
            var clientSecret = _configuration["MicrosoftGraph:ClientSecret"];
            var tenantId = _configuration["MicrosoftGraph:TenantId"];
            var fromUserEmail = _configuration["MicrosoftGraph:FromUserEmail"] ?? "system-bot@workflow.io.com";

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(tenantId))
            {
                _logger.LogWarning("Microsoft Graph API credentials not fully configured. Skipping API send request. (Local HTML preview generated successfully).");
                return;
            }

            try
            {
                _logger.LogInformation("Attempting Microsoft Graph authentication for tenant: {TenantId}", tenantId);
                
                // Get OAuth Token from login.microsoftonline.com
                var tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
                {
                    Content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id", clientId),
                        new KeyValuePair<string, string>("client_secret", clientSecret),
                        new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default")
                    })
                };

                using var tokenResponse = await _httpClient.SendAsync(tokenRequest);
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    var errorDetails = await tokenResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to acquire Graph API token: {ErrorDetails}", errorDetails);
                    return;
                }

                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(tokenJson);
                var accessToken = doc.RootElement.GetProperty("access_token").GetString();

                // Send email using acquired token
                var sendMailUrl = $"https://graph.microsoft.com/v1.0/users/{fromUserEmail}/sendMail";
                
                var recipientsList = new System.Collections.Generic.List<object>();
                foreach (var email in toAddresses)
                {
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        recipientsList.Add(new
                        {
                            emailAddress = new { address = email.Trim() }
                        });
                    }
                }

                var emailPayload = new
                {
                    message = new
                    {
                        subject = subject,
                        body = new
                        {
                            contentType = "HTML",
                            content = htmlBody
                        },
                        toRecipients = recipientsList
                    },
                    saveToSentItems = "false"
                };

                var jsonPayload = JsonSerializer.Serialize(emailPayload);
                var sendRequest = new HttpRequestMessage(HttpMethod.Post, sendMailUrl)
                {
                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                };
                sendRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using var sendResponse = await _httpClient.SendAsync(sendRequest);
                if (sendResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully sent daily sprint update email via Graph API to {Count} recipients.", toAddresses.Length);
                }
                else
                {
                    var errorResponse = await sendResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning("Microsoft Graph API returned error code {StatusCode}: {ErrorResponse}", sendResponse.StatusCode, errorResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while attempting to send email via Microsoft Graph API.");
            }
        }
    }
}
