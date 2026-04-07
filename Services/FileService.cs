using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Project3.DTOs;
using Project3.Helpers;
using Project3.Interfaces;
using Project3.Models;

namespace Project3.Services
{
    public class FileService : IFileService
    {        
        private readonly FileUploadSettings _settings;
        private readonly IAmazonS3 _s3Client;
                
        public FileService(IOptions<FileUploadSettings> options, IAmazonS3 s3Client)
        {
            _settings = options.Value;
            _s3Client = s3Client;
        }

        public async Task<FileUploadResponse> UploadFileAsync(Stream requestBody, string contentType)
        {
            // Check HTTP request content type
            if (!MultipartRequestHelper.IsMultipartContentType(contentType))
            {
                return new FileUploadResponse { IsSuccess = false, ErrorMessage = "Le type de contenu attendu est 'multipart/form-data'." };
            }

            // Read the boundary from the Content-Type header
            // Security : DoS mbLimit: 100
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(contentType), mbLimit: 100);

            // Use MultipartReader to stream data to a destination
            var reader = new MultipartReader(boundary, requestBody);

            var section = await reader.ReadNextSectionAsync();
            
            while (section != null)                        
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
                    !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    var originalFileName = contentDisposition.FileName.Value;
                    var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

                    // Security : Extension validation          
                    if (string.IsNullOrEmpty(extension) || !(FileValidationHelper.GetAllowedExtensions().Contains(extension, StringComparer.OrdinalIgnoreCase)))
                    {
                        return new FileUploadResponse { IsSuccess = false, ErrorMessage = "Type de fichier non autorisé" };
                    }

                    // Security : Generate new file name for S3 bucket
                    var s3Key = Guid.NewGuid().ToString() + extension;

                    // Security : Validate file signature (magic number)                    
                    if (!await FileValidationHelper.IsValidFile_MagicNumberAsync(section.Body, extension))
                    {
                        return new FileUploadResponse { IsSuccess = false, ErrorMessage = "Signature de fichier non autorisée" };
                    }

                    // Security : MIME type validation
                    var sectionContentType = section.ContentType;
                    if (string.IsNullOrEmpty(sectionContentType) ||
                        !FileValidationHelper.ExpectedMimeTypes.TryGetValue(extension, out var expectedMime) ||
                        !sectionContentType.StartsWith(expectedMime, StringComparison.OrdinalIgnoreCase))
                    {
                        return new FileUploadResponse { IsSuccess = false, ErrorMessage = "Le type de contenu (MIME) ne correspond pas à l'extension du fichier" };
                    }

                    try
                    {
                        // S3 Streaming
                        using (var fileTransferUtility = new TransferUtility(_s3Client))
                        {
                            var uploadRequest = new TransferUtilityUploadRequest
                            {
                                InputStream = section.Body,
                                Key = s3Key,
                                BucketName = _settings.AwsBucketName,
                                ContentType = section.ContentType
                            };

                            // S3 Upload
                            await fileTransferUtility.UploadAsync(uploadRequest);
                        }

                        return new FileUploadResponse
                        {
                            IsSuccess = true,
                            OriginalFileName = originalFileName,
                            SavedFileName = s3Key
                        };
                    }
                    catch (AmazonS3Exception ex)
                    {
                        return new FileUploadResponse { IsSuccess = false, ErrorMessage = $"Erreur S3 : {ex.Message}" };
                    }
                    catch (Exception)
                    {
                        return new FileUploadResponse { IsSuccess = false, ErrorMessage = "Une erreur serveur est survenue lors de l'envoi vers AWS S3" };
                    }
                }

                section = await reader.ReadNextSectionAsync();
            }

            return new FileUploadResponse { IsSuccess = false, ErrorMessage = "Aucun fichier valide n'a été trouvé" };
        }                
    }
}
