using CommentApi.Model.Dto;

namespace CommentApi.Services
{
    public interface ICommentService
    {
        Task<CommentResponseDto> CreateAsync(
            Guid taskId,
            Guid authorId,
            CreateCommentRequestDto request);

        Task<IEnumerable<CommentResponseDto>> GetTaskCommentsAsync(
            Guid taskId);

        Task<CommentResponseDto?> UpdateAsync(
            Guid commentId,
            Guid userId,
            UpdateCommentRequestDto request);

        Task<bool> DeleteAsync(
            Guid commentId,
            Guid userId);

        Task<CommentResponseDto?> AddReactionAsync(
            Guid commentId,
            Guid userId,
            string emoji);

        Task<CommentResponseDto?> RemoveReactionAsync(
            Guid commentId,
            Guid userId,
            string emoji);
    }
}
