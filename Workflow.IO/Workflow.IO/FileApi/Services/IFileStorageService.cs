using Microsoft.AspNetCore.Http;

namespace FileApi.Services
{
    public interface IFileStorageService
    {
        Task<StoredFileResult> SaveAsync(
            Guid taskId,
            IFormFile file,
            CancellationToken cancellationToken = default);

        Stream OpenRead(string storagePath);

        void Delete(string storagePath);
    }
}
