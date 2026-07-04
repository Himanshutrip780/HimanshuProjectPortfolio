using System;
using System.IO;
using System.Threading.Tasks;
using ATS.Application.Common.Interfaces;

namespace ATS.Infrastructure.Storage
{
    public class LocalFileStorageService : IStorageService
    {
        private readonly string _basePath;

        public LocalFileStorageService()
        {
            // Store inside wwwroot/uploads at application directory root
            _basePath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        }

        public async Task<string> UploadFileAsync(byte[] fileData, string fileName, string folderName)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var relativePath = Path.Combine("uploads", folderName, uniqueFileName);
            var absolutePath = Path.Combine(_basePath, relativePath);

            var directoryPath = Path.GetDirectoryName(absolutePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await File.WriteAllBytesAsync(absolutePath, fileData);
            return relativePath.Replace(Path.DirectorySeparatorChar, '/');
        }

        public async Task<byte[]> DownloadFileAsync(string filePath)
        {
            var absolutePath = Path.Combine(_basePath, filePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(absolutePath))
            {
                throw new FileNotFoundException("The file was not found on the server storage.", filePath);
            }

            return await File.ReadAllBytesAsync(absolutePath);
        }

        public Task DeleteFileAsync(string filePath)
        {
            var absolutePath = Path.Combine(_basePath, filePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
            return Task.CompletedTask;
        }
    }
}
