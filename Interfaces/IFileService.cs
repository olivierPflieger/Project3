using Project3.DTOs;

namespace Project3.Interfaces
{
    public interface IFileService
    {
        Task<FileUploadResponse> UploadFileAsync(Stream requestBody, string contentType);
    }
}
