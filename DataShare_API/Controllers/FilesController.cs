using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using DataShare_API.DTO;
using DataShare_API.Exceptions;
using DataShare_API.Filters;
using DataShare_API.Interfaces;
using DataShare_API.Models;
using DataShare_API.Utils;
using System.Net;
using System.Security.Claims;

namespace DataShare_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly FileUploadSettings _settings;

        // Injection des paramètres de configuration
        public FilesController(IFileService fileService, IOptions<FileUploadSettings> options)
        {
            _fileService = fileService;
            _settings = options.Value;
        }

        [HttpPost()]
        [DisableFormValueModelBinding]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> UploadFile()
        {
            try
            {
                // Security : Fix Kestrel server limit
                // Block request before to read it and stop transfer
                var maxRequestBodySizeFeature = HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
                if (maxRequestBodySizeFeature != null)
                {
                    maxRequestBodySizeFeature.MaxRequestBodySize = _settings.MaxFileSize;
                }

                // Security : Check File limit size
                if (Request.ContentLength.HasValue && Request.ContentLength.Value > _settings.MaxFileSize)
                {
                    return BadRequest($"La taille de la requête dépasse la limite autorisée de {_settings.MaxFileSize} octets.");
                }

                // Get Connected User Id
                int userId = 0;
                int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

                UploadFileResponse uploadFileResponse = await _fileService.UploadFileAsync(Request.Body, Request.ContentType, userId);
                
                return Ok(uploadFileResponse);
            }
            catch (CustomDatashareException ex)
            {
                return ResolveCustomException(ex);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("download/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadFile(string token, [FromBody] DownloadFileRequest request)
        {
            try
            {
                var result = await _fileService.DownloadFileAsync(token, request?.Password);

                return File(result.FileStream!, result.ContentType!, result.FileName!);
            }
            catch (CustomDatashareException ex)
            {
                return ResolveCustomException(ex);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFileMetaDataByToken(string token)
        {
            try
            {
                var fileMetaData = await _fileService.GetFileMetaDataByTokenAsync(token);

                var expirationDetails = FileUtils.CalculateExpirationDetails(fileMetaData.CreatedAt, fileMetaData.ExpirationDays);

                var fileMetaDataResponse = new FileMetaDataResponse
                {
                    OriginalFileName = fileMetaData.OriginalName,
                    FileSize = FileUtils.FormatFileSize(long.Parse(fileMetaData.Size)),
                    Extension = fileMetaData.Extension,
                    Token = fileMetaData.Token,
                    CreatedAt = fileMetaData.CreatedAt,
                    IsExpired = expirationDetails.isExpired,
                    ExpirationDays = expirationDetails.RemainingDays,
                    ExpirationDate = expirationDetails.ExpirationDate,
                    Tags = fileMetaData.Tags
                };

                return Ok(fileMetaDataResponse);
            }
            catch (CustomDatashareException ex)
            {
                return ResolveCustomException(ex);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllFiles()
        {
            try
            {
                // Get Connected User Id
                int userId = 0;
                int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

                var fileMetaDatas = await _fileService.GetAllFileMetaDatasAsync(userId);

                List<FileMetaDataResponse> FileMetaDataResponses = new List<FileMetaDataResponse>();
                foreach (var fileMetaData in fileMetaDatas)
                {
                    var expirationDetails = FileUtils.CalculateExpirationDetails(fileMetaData.CreatedAt, fileMetaData.ExpirationDays);

                    var fileMetaDataResponse = new FileMetaDataResponse
                    {
                        OriginalFileName = fileMetaData.OriginalName,
                        FileSize = FileUtils.FormatFileSize(long.Parse(fileMetaData.Size)),
                        Extension = fileMetaData.Extension,
                        Token = fileMetaData.Token,
                        CreatedAt = fileMetaData.CreatedAt,
                        IsExpired = expirationDetails.isExpired,
                        ExpirationDays = expirationDetails.RemainingDays,
                        ExpirationDate = expirationDetails.ExpirationDate,
                        Tags = fileMetaData.Tags
                    };
                    FileMetaDataResponses.Add(fileMetaDataResponse);
                }

                if (FileMetaDataResponses.Count == 0)
                {
                    return NotFound(new { message = "Aucun fichier trouvé pour cet utilisateur" });
                }

                return Ok(FileMetaDataResponses);
            }            
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{token}")]
        public async Task<IActionResult> DeleteFile(string token)
        {
            try
            {
                int userId = 0;
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId) || userId == 0)
                {
                    return Unauthorized(new { message = "Utilisateur non authentifié." });
                }

                bool isDeleted = await _fileService.DeleteFileAsync(token, userId);

                if (!isDeleted)
                    return BadRequest(new { message = "Impossible de supprimer ce fichier." });

                return Ok(new { message = "Fichier supprimé avec succès." });
            }
            catch (CustomDatashareException ex)
            {
                return ResolveCustomException(ex);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private IActionResult ResolveCustomException(CustomDatashareException ex)
        {
            switch (ex.Code)
            {
                case HttpStatusCode.NotFound:
                    return NotFound(new { message = ex.Message });
                case HttpStatusCode.Conflict:
                    return Conflict(new { message = ex.Message });
                case HttpStatusCode.BadRequest:
                    return BadRequest(new { message = ex.Message });
                default:
                    return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
