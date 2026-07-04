namespace Workflow.IO.Shared.Contracts
{
    public class ErrorResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string TraceId { get; set; } = string.Empty;

        public IDictionary<string, string[]>? Errors { get; set; }

        public static ErrorResponse Create(
            string message,
            string traceId,
            IDictionary<string, string[]>? errors = null)
        {
            return new ErrorResponse
            {
                Success = false,
                Message = message,
                TraceId = traceId,
                Errors = errors
            };
        }
    }
}
