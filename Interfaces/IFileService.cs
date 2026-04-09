using Project3.ViewModels;
using Project3.Models;

namespace Project3.Interfaces
{
    public interface IFileService
    {
        Task<UploadFileMetaDataViewModel> UploadFileAsync(Stream requestBody, string contentType, int userId);

        Task<List<FileMetaData>> GetAllFileMetaDatasAsync(int userId);
        
        Task<FileMetaData> GetFileMetaDataByTokenAsync(string token);
    }
}
