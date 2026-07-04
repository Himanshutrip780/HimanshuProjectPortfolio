namespace TaskApi.Clients
{
    public interface IProjectMetadataClient
    {
        Task<ProjectMetadataDto?> GetProjectMetadataAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);
    }

    public class ProjectMetadataDto
    {
        public Guid ProjectId { get; set; }

        public string Key { get; set; } = string.Empty;
    }
}
