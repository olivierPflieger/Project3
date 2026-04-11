using DataShare_API.DTO;
using DataShare_API.Models;

namespace DataShare_API.Interfaces
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
