namespace TaskApi.Clients
{
    public interface IProjectAccessClient
    {
        Task<bool> IsProjectMemberAsync(
            Guid projectId,
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<bool> CanContributeAsync(
            Guid projectId,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
