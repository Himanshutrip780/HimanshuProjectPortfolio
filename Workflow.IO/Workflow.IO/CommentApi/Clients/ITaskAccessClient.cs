namespace CommentApi.Clients
{
    public interface ITaskAccessClient
    {
        Task<TaskAccessDto?> GetAccessibleTaskAsync(
            Guid taskId,
            CancellationToken cancellationToken = default);
    }
}
