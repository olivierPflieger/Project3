using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Project3.Filters;
using Project3.Interfaces;
using Project3.Models;

namespace Project3.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FileController : ControllerBase
    {        
        private readonly IFileService _fileService;
        private readonly FileUploadSettings _settings;

        // Injection des paramètres de configuration
        public FileController(IFileService fileService, IOptions<FileUploadSettings> options)
        {
            _fileService = fileService;
            _settings = options.Value;
        }

        [HttpPost("upload")]
        [DisableFormValueModelBinding]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> UploadFile()
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
                        
            var result = await _fileService.UploadFileAsync(Request.Body, Request.ContentType);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new
            {
                Message = "Fichier uploadé avec succès.",
                OriginalName = result.OriginalFileName,
                SavedName = result.SavedFileName
            });
        }                
    }
}
