using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Project3.Helpers;
using Project3.Interfaces;
using Project3.Models;
using Project3.Utils;
using Project3.DTO;
using System.Globalization;

namespace Project3.Services
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
            {
                return new UploadFileResponse { IsSuccess = false, Message = "Le type de contenu attendu est 'multipart/form-data'." };
            }

            // Read the boundary from the Content-Type header
            // Security : DoS mbLimit: 100
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(contentType), mbLimit: 100);

            // Use MultipartReader to stream data to a destination
            var reader = new MultipartReader(boundary, requestBody);

            var section = await reader.ReadNextSectionAsync();

            // Additional parameters (ex: password, expiration, tags)
            string passwordHash = string.Empty;
            int expiration = 7;
            string[] tags = Array.Empty<string>();

            while (section != null)                        
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                // Additional parameters from body (ex: password, tags, etc.)
                if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data"))                    
                {
                    if (string.IsNullOrEmpty(contentDisposition.FileName.Value))
                    {
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
                                                
                        using var streamReader = new StreamReader(section.Body, System.Text.Encoding.UTF8);
                        var value = await streamReader.ReadToEndAsync();

                        if (key.Equals("password", StringComparison.OrdinalIgnoreCase))
                        {                            
                            if (!string.IsNullOrEmpty(value))
                                passwordHash = BCrypt.Net.BCrypt.HashPassword(value);
                        }

                        if (key.Equals("expiration", StringComparison.OrdinalIgnoreCase))
                        {
                            int.TryParse(value, out expiration);
                        }

                        if (key.Equals("tags", StringComparison.OrdinalIgnoreCase))
                        {
                            tags = value.Trim().Split(",");
                        }
                    }
                }

                // Streaming
                if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
                    !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    var originalFileName = contentDisposition.FileName.Value;
                    var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

                    // Security : Extension validation          
                    if (string.IsNullOrEmpty(extension) || !(FileValidationHelper.GetAllowedExtensions().Contains(extension, StringComparer.OrdinalIgnoreCase)))
                    {
                        return new UploadFileResponse { IsSuccess = false, Message = "Type de fichier non autorisé" };
                    }

                    // Security : Generate new file name for S3 bucket
                    var token = Guid.NewGuid().ToString();
                    var generatedFileName = String.Format("{0}{1}", token, extension);

                    // Security : Validate file signature (magic number)                    
                    if (!await FileValidationHelper.IsValidFile_MagicNumberAsync(section.Body, extension))
                    {
                        return new UploadFileResponse { IsSuccess = false, Message = "Signature de fichier non autorisée" };
                    }

                    // Security : MIME type validation
                    var sectionContentType = section.ContentType;
                    if (string.IsNullOrEmpty(sectionContentType) ||
                        !FileValidationHelper.ExpectedMimeTypes.TryGetValue(extension, out var expectedMime) ||
                        !sectionContentType.StartsWith(expectedMime, StringComparison.OrdinalIgnoreCase))
                    {
                        return new UploadFileResponse { IsSuccess = false, Message = "Le type de contenu (MIME) ne correspond pas à l'extension du fichier" };
                    }

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
                                Password = passwordHash,
                                CreatedDate = DateTime.UtcNow,
                                Expiration = expiration,
                                Tags = tags,
                                UserId = userId
                            };

                            CreateFileMetaDataAsync(fileMetaData).Wait();

                            // Finally return response
                            return new UploadFileResponse
                            {
                                IsSuccess = true,
                                Message = "Fichier correctement téléversé",
                                OriginalFileName = originalFileName,
                                Token = token,
                                Extension = extension,                                
                                FileSize = FileUtils.FormatFileSize(s3ObjectMetadata.ContentLength),
                                CreatedDate = fileMetaData.CreatedDate,
                                Expiration = expiration
                            };
                        }
                    }
                    catch (AmazonS3Exception ex)
                    {
                        return new UploadFileResponse { IsSuccess = false, Message = $"Erreur S3 : {ex.Message}" };
                    }
                    catch (Exception ex)
                    {
                        if (!string.IsNullOrEmpty(ex.Message))
                        {
                            return new UploadFileResponse { IsSuccess = false, Message = $"Erreur serveur : {ex.Message}" };
                        }
                        else
                        {
                            return new UploadFileResponse { IsSuccess = false, Message = "Une erreur serveur est survenue lors de l'envoi du fichier" };
                        }
                    }
                }
                                
                section = await reader.ReadNextSectionAsync();
            }

            return new UploadFileResponse { IsSuccess = false, Message = "Aucun fichier valide n'a été trouvé" };
        }

        public async Task<DownloadFileResponse> DownloadFileAsync(string token, string? password)
        {
            // Lecture du fichier en base
            var fileMetaData = await _context.FileMetaDatas.SingleOrDefaultAsync(f => f.Token == token);

            if (fileMetaData == null)
                return new DownloadFileResponse { Success = false, ErrorCode = 404, ErrorMessage = "Fichier introuvable ou token invalide." };

            if (!string.IsNullOrEmpty(fileMetaData.Password))
            {
                if (string.IsNullOrEmpty(password))                
                    return new DownloadFileResponse { Success = false, ErrorCode = 401, ErrorMessage = "Ce fichier est protégé par un mot de passe." };
                
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, fileMetaData.Password);
                if (!isPasswordValid)                
                    return new DownloadFileResponse { Success = false, ErrorCode = 401, ErrorMessage = "Mot de passe incorrect." };                
            }

            // Récupération depuis AWS S3
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _settings.AwsBucketName,
                    Key = String.Format("{0}{1}", fileMetaData.Token, fileMetaData.Extension)
                };

                var response = await _s3Client.GetObjectAsync(request);

                // On retourne le flux. Le contrôleur s'occupera de le lire et de le fermer.
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
                return new DownloadFileResponse { Success = false, ErrorCode = 500, ErrorMessage = $"Erreur AWS : {ex.Message}" };
            }
        }

        public async Task<FileMetaData> GetFileMetaDataByTokenAsync(string token)
        {
            return await _context.FileMetaDatas.FirstOrDefaultAsync(f=>f.Token == token);
        }

        public async Task<List<FileMetaData>> GetAllFileMetaDatasAsync(int userId)
        {
            return await _context.FileMetaDatas.Where(f => f.UserId == userId).ToListAsync();
        }

        private async Task<int> CreateFileMetaDataAsync(FileMetaData fileMetaData)
        {
            _context.FileMetaDatas.Add(fileMetaData);
            return await _context.SaveChangesAsync();
        }                
    }
}
