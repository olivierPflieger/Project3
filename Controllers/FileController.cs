using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Project3.ViewModels;
using Project3.Filters;
using Project3.Interfaces;
using Project3.Models;
using System.Security.Claims;

namespace Project3.Controllers
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

            FileMetaDataViewModel fileMetaData = await _fileService.UploadFileAsync(Request.Body, Request.ContentType, userId);
            
            if (!fileMetaData.IsSuccess)
            {
                return BadRequest(fileMetaData.ErrorMessage);
            }

            return Ok(fileMetaData);
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllFilesMetaDatas()
        {
            try
            {
                var fileMetaDatas = await _fileService.GetAllFileMetaDatasAsync();
                return Ok(fileMetaDatas);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Une erreur s'est produite durant la lecture des fichiers: {ex.Message}";
                return StatusCode(500, new { message = errorMessage });
            }
        }
    }
}
