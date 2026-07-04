namespace Workflow.IO.Shared.IntegrationEvents
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "localhost";

        public int Port { get; set; } = 5672;

        public string UserName { get; set; } = "guest";

        public string Password { get; set; } = "guest";

        public string ExchangeName { get; set; } = "workflow.io.events";

        public int MaxRetries { get; set; } = 5;

        public int DispatchBatchSize { get; set; } = 20;

        public int DispatchIntervalSeconds { get; set; } = 5;
    }
}
