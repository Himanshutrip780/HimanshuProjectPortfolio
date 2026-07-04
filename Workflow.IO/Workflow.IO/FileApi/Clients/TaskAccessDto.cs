using System.Text.Json.Serialization;

namespace FileApi.Clients
{
    public class TaskAccessDto
    {
        [JsonPropertyName("taskId")]
        public Guid TaskId { get; set; }

        [JsonPropertyName("projectId")]
        public Guid ProjectId { get; set; }

        [JsonPropertyName("assigneeId")]
        public Guid? AssigneeId { get; set; }

        [JsonPropertyName("reporterId")]
        public Guid ReporterId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }
}
