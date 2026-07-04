using CommentApi.Model.Domain.Entities;

namespace CommentApi.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment> CreateAsync(Comment comment);

        Task<IEnumerable<Comment>> GetByTaskIdAsync(Guid taskId);

        Task<Comment?> GetByIdAsync(Guid commentId);

        Task AddMentionsAsync(
            IEnumerable<CommentMention> mentions);

        Task<IEnumerable<CommentMention>> GetMentionsAsync(
            Guid commentId);

        Task ReplaceMentionsAsync(
            Guid commentId,
            IEnumerable<CommentMention> mentions);

        Task SaveChangesAsync();
    }
}
