using Microsoft.AspNetCore.Http;

namespace FileApi.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _rootPath;

        public LocalFileStorageService(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            var configuredRoot =
                configuration["Storage:RootPath"] ??
                "Uploads";

            _rootPath =
                Path.IsPathRooted(configuredRoot)
                    ? configuredRoot
                    : Path.Combine(
                        environment.ContentRootPath,
                        configuredRoot);
        }

        public async Task<StoredFileResult> SaveAsync(
            Guid taskId,
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            var taskFolder =
                Path.Combine(
                    _rootPath,
                    taskId.ToString("N"));

            Directory.CreateDirectory(taskFolder);

            var extension =
                Path.GetExtension(file.FileName);

            var storedFileName =
                $"{Guid.NewGuid():N}{extension}";

            var storagePath =
                Path.Combine(
                    taskFolder,
                    storedFileName);

            await using var stream =
                new FileStream(
                    storagePath,
                    FileMode.CreateNew);

            await file.CopyToAsync(
                stream,
                cancellationToken);

            return new StoredFileResult
            {
                StoredFileName = storedFileName,
                StoragePath = storagePath
            };
        }

        public Stream OpenRead(string storagePath)
        {
            return File.OpenRead(storagePath);
        }

        public void Delete(string storagePath)
        {
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
            }
        }
    }
}
