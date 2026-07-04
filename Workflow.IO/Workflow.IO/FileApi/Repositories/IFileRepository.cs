using FileApi.Model.Domain.Entities;

namespace FileApi.Repositories
{
    public interface IFileRepository
    {
        Task<FileAttachment> CreateAsync(
            FileAttachment attachment);

        Task<IEnumerable<FileAttachment>> GetTaskAttachmentsAsync(
            Guid taskId);

        Task<FileAttachment?> GetByIdAsync(
            Guid fileAttachmentId);

        Task SaveChangesAsync();
    }
}
