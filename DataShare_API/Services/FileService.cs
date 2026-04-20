using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using DataShare_API.DTO;
using DataShare_API.Exceptions;
using DataShare_API.Helpers;
using DataShare_API.Interfaces;
using DataShare_API.Models;
using DataShare_API.Utils;
using System.Net;

namespace DataShare_API.Services
{
    public class FileService : IFileService
    {        
        private readonly FileUploadSettings _settings;
        private readonly IAmazonS3 _s3Client;
        private readonly AppDbContext _context;

        public FileService(IOptions<FileUploadSettings> options, IAmazonS3 s3Client, AppDbContext context)
        {
            _settings = options.Value;
            _s3Client = s3Client;
            _context = context;
        }

        public async Task<UploadFileResponse> UploadFileAsync(Stream requestBody, string contentType, int userId)
        {
            // Check HTTP request content type
            if (!MultipartRequestHelper.IsMultipartContentType(contentType))
                throw new CustomDatashareException(HttpStatusCode.BadRequest, "Le type de contenu attendu est 'multipart/form-data'.");

            // Get boundary from the Content-Type header
            // Security : DoS mbLimit: 100
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(contentType), mbLimit: 100);

            // Use MultipartReader to stream data to a destination
            var reader = new MultipartReader(boundary, requestBody);
            var section = await reader.ReadNextSectionAsync();

            // Additional inputs parameters from body
            var uploadMetadata = new UploadMetadata();

            var fileMetaData = (FileMetaData?)null;
            while (section != null)                        
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                // Extract additional parameters from body (ex: password, tags, etc.)
                if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data"))                    
                {
                    // Extract additional parameters from body(ex: password, tags, etc.)
                    if (string.IsNullOrEmpty(contentDisposition.FileName.Value))
                    {
                        await ExtractFormMetadataAsync(section, contentDisposition, uploadMetadata);
                    }
                    // Streaming & Processing file
                    else if (!string.IsNullOrEmpty(contentDisposition.FileName.Value))
                    {
                        fileMetaData = await ProcessAndUploadFileAsync(section, contentDisposition.FileName.Value, uploadMetadata, userId);
                    }
                }
                    
                section = await reader.ReadNextSectionAsync();
            }

            if (fileMetaData == null)
                throw new CustomDatashareException(HttpStatusCode.BadRequest, "Aucun fichier valide n'a été trouvé");

            // Injection des paramètres additionnels
            fileMetaData.ExpirationDays = uploadMetadata.ExpirationDays;            
            fileMetaData.Password = uploadMetadata.PasswordHash;
            fileMetaData.Tags = uploadMetadata.Tags;

            try
            {
                await CreateFileMetaDataAsync(fileMetaData);
            } 
            catch (Exception ex)
            {
                // L'insertion en base a échoué, on supprime le fichier orphelin sur S3
                try
                {
                    var deleteRequest = new DeleteObjectRequest
                    {
                        BucketName = _settings.AwsBucketName,
                        Key = String.Format("{0}{1}", fileMetaData.Token, fileMetaData.Extension)
                    };
                    await _s3Client.DeleteObjectAsync(deleteRequest);
                }
                catch
                {
                    throw new CustomDatashareException(HttpStatusCode.BadRequest, "Erreur d'insertion en base de données");
                }
            }

            // Finally return response
            return new UploadFileResponse
            {
                OriginalFileName = fileMetaData.OriginalName,
                Token = fileMetaData.Token,
                Extension = fileMetaData.Extension,
                FileSize = FileUtils.FormatFileSize(long.Parse(fileMetaData.Size)),
                CreatedAt = fileMetaData.CreatedAt,
                ExpirationDays = fileMetaData.ExpirationDays
            };
        }

        public async Task<DownloadFileResponse> DownloadFileAsync(string token, string? password)
        {
            // Lecture du fichier en base
            var fileMetaData = await _context.FileMetaDatas.SingleOrDefaultAsync(f => f.Token == token);

            if (fileMetaData == null)
                throw new CustomDatashareException(HttpStatusCode.NotFound, "Fichier introuvable ou token invalide");

            if (!string.IsNullOrEmpty(fileMetaData.Password))
            {
                if (string.IsNullOrEmpty(password))                
                    throw new CustomDatashareException(HttpStatusCode.Conflict, "Ce fichier est protégé par un mot de passe");
                
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, fileMetaData.Password);
                if (!isPasswordValid)                
                    throw new CustomDatashareException(HttpStatusCode.Conflict, "Mot de passe incorrect");
            }

            var expirationDetails = FileUtils.CalculateExpirationDetails(fileMetaData.CreatedAt, fileMetaData.ExpirationDays);
            if (expirationDetails.IsExpired)
                throw new CustomDatashareException(HttpStatusCode.Gone, $"Ce fichier a expiré et n'est plus disponible en téléchargement.");

            // Download depuis AWS S3
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _settings.AwsBucketName,
                    Key = String.Format("{0}{1}", fileMetaData.Token, fileMetaData.Extension)
                };

                var response = await _s3Client.GetObjectAsync(request);

                return new DownloadFileResponse
                {
                    Success = true,
                    FileStream = response.ResponseStream,
                    ContentType = response.Headers.ContentType,
                    FileName = fileMetaData.OriginalName
                };
            }
            catch (AmazonS3Exception ex)
            {
                throw new CustomDatashareException(HttpStatusCode.InternalServerError, $"Erreur lors de la récupération du fichier depuis AWS S3 : {ex.Message}");
            }
        }

        public async Task<FileMetaData> GetFileMetaDataByTokenAsync(string token)
        {
            var fileMetaData = await _context.FileMetaDatas.FirstOrDefaultAsync(f=>f.Token == token);

            if (fileMetaData == null)
                throw new CustomDatashareException(HttpStatusCode.NotFound, "Fichier introuvable");

            return fileMetaData;
        }

        public async Task<List<FileMetaData>> GetAllFileMetaDatasAsync(int userId)
        {
            return await _context.FileMetaDatas.Where(f => f.UserId == userId).ToListAsync();
        }

        public async Task<bool> DeleteFileAsync(string token, int userId)

        {
            var fileMetaData = await _context.FileMetaDatas.SingleOrDefaultAsync(f => f.Token == token);

            // Security check - File not found or user is not the owner
            if (fileMetaData == null || fileMetaData.UserId != userId)
                throw new CustomDatashareException(HttpStatusCode.NotFound,"Suppression impossible, ce fichier n'existe pas ou ne vous appartient pas");

            // Delete from S3
            try
            {
                var s3Key = String.Format("{0}{1}", fileMetaData.Token, fileMetaData.Extension);
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _settings.AwsBucketName,
                    Key = s3Key
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
            }
            catch (AmazonS3Exception ex)
            {
                throw new CustomDatashareException(HttpStatusCode.InternalServerError, $"Erreur lors de la suppression du fichier sur AWS S3 : {ex.Message}");
            }

            // Delete from Database
            _context.FileMetaDatas.Remove(fileMetaData);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<int> CreateFileMetaDataAsync(FileMetaData fileMetaData)
        {
            _context.FileMetaDatas.Add(fileMetaData);
            return await _context.SaveChangesAsync();
        }

        // File Upload : Extract additional parameters from body (ex: password, tags, etc.)
        private async Task ExtractFormMetadataAsync(MultipartSection section, ContentDispositionHeaderValue contentDisposition, UploadMetadata uploadMetadata)
        {
            var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;

            using var streamReader = new StreamReader(section.Body, System.Text.Encoding.UTF8);
            var value = await streamReader.ReadToEndAsync();

            if (key.Equals("password", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(value))
            {
                uploadMetadata.PasswordHash = BCrypt.Net.BCrypt.HashPassword(value);
            }
            else if (key.Equals("expiration", StringComparison.OrdinalIgnoreCase))
            {                
                int.TryParse(value, out int expirationDays);
                if (expirationDays == 0)
                    throw new CustomDatashareException(HttpStatusCode.BadRequest, "Problème avec la valeur d'expiration");
                
                uploadMetadata.ExpirationDays = expirationDays;
            }
            else if (key.Equals("tags", StringComparison.OrdinalIgnoreCase))
            {
                uploadMetadata.Tags = value.Trim().Split(",");
            }
        }

        // File Upload : Validate file security (extension, magic number, MIME type)
        private async Task ValidateFileSecurityAsync(Stream fileStream, string extension, string sectionContentType)
        {
            // Extension validation          
            if (string.IsNullOrEmpty(extension) || !(FileValidationHelper.GetAllowedExtensions().Contains(extension, StringComparer.OrdinalIgnoreCase)))
            {
                throw new CustomDatashareException(HttpStatusCode.BadRequest, "Type de fichier non autorisé");
            }

            // File signature (magic number)                    
            if (!await FileValidationHelper.IsValidFile_MagicNumberAsync(fileStream, extension))
            {
                throw new CustomDatashareException(HttpStatusCode.BadRequest, "Signature de fichier non autorisée");
            }

            // MIME type validation
            if (string.IsNullOrEmpty(sectionContentType) ||
                !FileValidationHelper.ExpectedMimeTypes.TryGetValue(extension, out var expectedMime) ||
                !sectionContentType.StartsWith(expectedMime, StringComparison.OrdinalIgnoreCase))
            {
                throw new CustomDatashareException(HttpStatusCode.BadRequest, "Le type de contenu (MIME) ne correspond pas à l'extension du fichier");
            }
        }

        // File Upload : Process file and upload to S3
        private async Task<FileMetaData> ProcessAndUploadFileAsync(MultipartSection section, string originalFileName, UploadMetadata metadata, int userId)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

            await ValidateFileSecurityAsync(section.Body, extension, section.ContentType);

            // Security : Generate new file name for S3 bucket
            var token = Guid.NewGuid().ToString();
            var generatedFileName = String.Format("{0}{1}", token, extension);

            try
            {
                // S3 Streaming
                using (var fileTransferUtility = new TransferUtility(_s3Client))
                {
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = section.Body,
                        Key = generatedFileName,
                        BucketName = _settings.AwsBucketName,
                        ContentType = section.ContentType
                    };

                    // S3 Upload
                    await fileTransferUtility.UploadAsync(uploadRequest);

                    // Get file metadata from S3 to retrieve the file size
                    var s3ObjectMetadata = await _s3Client.GetObjectMetadataAsync(_settings.AwsBucketName, generatedFileName);

                    // Create file metadata record in database
                    var fileMetaData = new Models.FileMetaData
                    {
                        OriginalName = originalFileName,
                        Token = token,
                        Extension = extension,
                        Size = s3ObjectMetadata.ContentLength.ToString(),
                        Password = metadata.PasswordHash,
                        CreatedAt = DateTime.UtcNow,
                        ExpirationDays = metadata.ExpirationDays,
                        Tags = metadata.Tags,
                        UserId = userId
                    };

                    return fileMetaData;                    
                }
            }
            catch (Exception ex)
            {
                throw new CustomDatashareException(HttpStatusCode.InternalServerError, $"Erreur lors de l'envoi du fichier vers AWS S3 : {ex.Message}");
            }
        }

        private class UploadMetadata
        {
            public string PasswordHash { get; set; } = string.Empty;
            public int ExpirationDays { get; set; } = 7;
            public string[] Tags { get; set; } = Array.Empty<string>();
        }
    }        
}
