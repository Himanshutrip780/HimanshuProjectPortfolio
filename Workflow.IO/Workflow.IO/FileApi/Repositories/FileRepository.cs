using FileApi.Data;
using FileApi.Model.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileApi.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly FileDbContext _context;

        public FileRepository(FileDbContext context)
        {
            _context = context;
        }

        public async Task<FileAttachment> CreateAsync(
            FileAttachment attachment)
        {
            await _context.FileAttachments.AddAsync(attachment);

            return attachment;
        }

        public async Task<IEnumerable<FileAttachment>> GetTaskAttachmentsAsync(
            Guid taskId)
        {
            return await _context.FileAttachments
                .AsNoTracking()
                .Where(x =>
                    x.TaskId == taskId &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<FileAttachment?> GetByIdAsync(
            Guid fileAttachmentId)
        {
            return await _context.FileAttachments
                .FirstOrDefaultAsync(x =>
                    x.FileAttachmentId == fileAttachmentId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
