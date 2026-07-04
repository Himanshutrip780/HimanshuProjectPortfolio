using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GatewayApi.Services
{
    public class KeepAliveService : BackgroundService
    {
        private readonly ILogger<KeepAliveService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string? _externalUrl;

        public KeepAliveService(ILogger<KeepAliveService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            
            // Render.com automatically provides this environment variable to web services
            _externalUrl = Environment.GetEnvironmentVariable("RENDER_EXTERNAL_URL");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("KeepAliveService started. Will ping gateway and all downstream services every 10 minutes to prevent sleep.");

            // Ping every 10 minutes (Render sleeps after 15 minutes of inactivity)
            var pingInterval = TimeSpan.FromMinutes(10);
            
            using var timer = new PeriodicTimer(pingInterval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var urlsToPing = new List<string>();

                    if (!string.IsNullOrWhiteSpace(_externalUrl))
                    {
                        urlsToPing.Add($"{_externalUrl.TrimEnd('/')}/ready");
                    }

                    // Extract all downstream microservice URLs from YARP configuration
                    var clusters = _configuration.GetSection("ReverseProxy:Clusters").GetChildren();
                    foreach (var cluster in clusters)
                    {
                        var destinations = cluster.GetSection("Destinations").GetChildren();
                        foreach (var dest in destinations)
                        {
                            var address = dest["Address"];
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                urlsToPing.Add($"{address.TrimEnd('/')}/ready");
                            }
                        }
                    }

                    // Remove duplicates just in case
                    urlsToPing = urlsToPing.Distinct().ToList();

                    _logger.LogInformation("KeepAliveService: Pinging {Count} services to stay awake...", urlsToPing.Count);

                    var pingTasks = urlsToPing.Select(async url =>
                    {
                        try
                        {
                            var response = await client.GetAsync(url, stoppingToken);
                            if (!response.IsSuccessStatusCode)
                            {
                                _logger.LogWarning("KeepAliveService: Ping failed for {Url}. Status code: {StatusCode}", url, response.StatusCode);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("KeepAliveService: Ping failed for {Url}. Error: {Message}", url, ex.Message);
                        }
                    });

                    await Task.WhenAll(pingTasks);
                    _logger.LogInformation("KeepAliveService: Finished pinging all services.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "KeepAliveService: An error occurred while pinging applications.");
                }
            }
        }
    }
}
