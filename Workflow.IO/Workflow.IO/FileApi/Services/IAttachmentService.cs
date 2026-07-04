using FileApi.Model.Dto;
using Microsoft.AspNetCore.Http;

namespace FileApi.Services
{
    public interface IAttachmentService
    {
        Task<FileAttachmentResponseDto> UploadAsync(
            Guid taskId,
            Guid uploadedById,
            IFormFile file,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<FileAttachmentResponseDto>> GetTaskAttachmentsAsync(
            Guid taskId);

        Task<AttachmentDownloadResult?> DownloadAsync(
            Guid fileAttachmentId);

        Task<bool> DeleteAsync(
            Guid fileAttachmentId,
            Guid userId);
    }
}
