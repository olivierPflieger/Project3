using Project3.DTO;
using Project3.Models;

namespace Project3.Interfaces
{
    public interface IFileService
    {
        Task<UploadFileResponse> UploadFileAsync(Stream requestBody, string contentType, int userId);

        Task<DownloadFileResponse> DownloadFileAsync(string token, string? password);

        Task<List<FileMetaData>> GetAllFileMetaDatasAsync(int userId);
        
        Task<FileMetaData> GetFileMetaDataByTokenAsync(string token);

        Task<bool> DeleteFileAsync(string token, int userId);
    }
}
