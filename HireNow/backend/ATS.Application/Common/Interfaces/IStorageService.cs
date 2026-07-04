using System.Threading.Tasks;

namespace ATS.Application.Common.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(byte[] fileData, string fileName, string folderName);
        Task<byte[]> DownloadFileAsync(string filePath);
        Task DeleteFileAsync(string filePath);
    }
}
