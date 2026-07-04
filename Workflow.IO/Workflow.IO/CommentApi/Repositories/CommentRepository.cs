using CommentApi.Data;
using CommentApi.Model.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommentApi.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly CommentDbContext _context;

        public CommentRepository(CommentDbContext context)
        {
            _context = context;
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);

            return comment;
        }

        public async Task<IEnumerable<Comment>> GetByTaskIdAsync(
            Guid taskId)
        {
            return await _context.Comments
                .Include(x => x.Reactions)
                .AsNoTracking()
                .Where(x => x.TaskId == taskId && !x.IsDeleted)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(Guid commentId)
        {
            return await _context.Comments
                .Include(x => x.Reactions)
                .FirstOrDefaultAsync(
                    x => x.CommentId == commentId);
        }

        public async Task AddMentionsAsync(
            IEnumerable<CommentMention> mentions)
        {
            await _context.CommentMentions.AddRangeAsync(mentions);
        }

        public async Task<IEnumerable<CommentMention>> GetMentionsAsync(
            Guid commentId)
        {
            return await _context.CommentMentions
                .AsNoTracking()
                .Where(x => x.CommentId == commentId)
                .ToListAsync();
        }

        public async Task ReplaceMentionsAsync(
            Guid commentId,
            IEnumerable<CommentMention> mentions)
        {
            var existingMentions =
                await _context.CommentMentions
                    .Where(x => x.CommentId == commentId)
                    .ToListAsync();

            _context.CommentMentions.RemoveRange(existingMentions);

            await _context.CommentMentions.AddRangeAsync(mentions);
        }

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
