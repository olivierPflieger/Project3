using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Project3.ViewModels;
using Project3.Filters;
using Project3.Interfaces;
using Project3.Models;
using System.Security.Claims;
using Project3.Utils;

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

            UploadFileMetaDataViewModel fileMetaData = await _fileService.UploadFileAsync(Request.Body, Request.ContentType, userId);
            
            if (!fileMetaData.IsSuccess)
            {
                return BadRequest(fileMetaData.Message);
            }

            return Ok(fileMetaData);
        }

        [HttpGet("{token}")]
        public async Task<IActionResult> GetFileByToken(string token)
        {
            // 1. Appel de la méthode avec le paramètre reçu par l'URL
            var fileMetaData = await _fileService.GetFileMetaDataByTokenAsync(token);

            // 2. Vérification si le fichier existe
            if (fileMetaData == null)
            {
                return NotFound(new { message = "Fichier introuvable" });
            }
                        
            var fileMetaDataViewModel = new FileMetaDataViewModel
            {
                OriginalFileName = fileMetaData.OriginalName,
                FileSize = fileMetaData.Size,
                Extension = fileMetaData.Extension,
                Token = fileMetaData.Token,
                CreatedDate = fileMetaData.CreatedDate,
                Expiration = fileMetaData.Expiration
            };

            return Ok(fileMetaDataViewModel);
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllFilesMetaDatas()
        {
            try
            {
                // Get Connected User Id
                int userId = 0;
                int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

                var fileMetaDatas = await _fileService.GetAllFileMetaDatasAsync(userId);

                List<FileMetaDataViewModel> fileMetaDataViewModels = fileMetaDatas.Select(f => new FileMetaDataViewModel
                {
                    OriginalFileName = f.OriginalName,
                    FileSize = FileUtils.FormatFileSize(long.Parse(f.Size)),
                    Extension = f.Extension,
                    Token = f.Token,
                    CreatedDate = f.CreatedDate,
                    Expiration = f.Expiration,
                    Tags = f.Tags
                }).ToList();

                if (fileMetaDataViewModels.Count == 0)
                {
                    return NotFound(new { message = "Aucun fichier trouvé pour cet utilisateur" });
                }

                return Ok(fileMetaDataViewModels);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Une erreur s'est produite durant la lecture des fichiers: {ex.Message}";
                return StatusCode(500, new { message = errorMessage });
            }
        }
    }
}
