using DataShare_API.Controllers;
using DataShare_API.DTO;
using DataShare_API.Exceptions;
using DataShare_API.Interfaces;
using DataShare_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Security.Claims;

namespace DataShare_API.Tests.Controllers
{
    public class FilesControllerTests
    {
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IOptions<FileUploadSettings>> _settingsMock;
        private readonly FilesController _filesController;

        public FilesControllerTests()
        {
            _fileServiceMock = new Mock<IFileService>();

            // Setup option FileUploadSettings
            _settingsMock = new Mock<IOptions<FileUploadSettings>>();
            _settingsMock.Setup(s => s.Value).Returns(new FileUploadSettings 
            { 
                MaxFileSize = 5000000 // Limite de 5Mo pour le test 
            });

            _filesController = new FilesController(_fileServiceMock.Object, _settingsMock.Object);

            // Simuler un utilisateur connecté avec un ClaimTypes.NameIdentifier = 1
            SetupMockUserContext(1);
        }

        private void SetupMockUserContext(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Simulation du HttpContext et des propriétés de requête
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            // Ajout ou Simulation des Features utilisés dans FilesController.UploadFile()
            var requestFeatureMock = new Mock<IHttpMaxRequestBodySizeFeature>();
            requestFeatureMock.SetupProperty(x => x.MaxRequestBodySize, 0); 
            httpContext.Features.Set<IHttpMaxRequestBodySizeFeature>(requestFeatureMock.Object);

            httpContext.Request.ContentType = "multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW";

            _filesController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task UploadFile_ValidFile_ReturnsOk()
        {
            // Arrange
            var expectedResponse = new UploadFileResponse
            {
                OriginalFileName = "test.pdf",
                Extension = ".pdf",
                Token = Guid.NewGuid().ToString()
            };

            _fileServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), 1))
                            .ReturnsAsync(expectedResponse);

            // Act
            var result = await _filesController.UploadFile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task UploadFile_RequestTooLarge_ReturnsBadRequest()
        {
            // Arrange
            // On manipule le context simulé pour lui donner une taille supérieure à 5Mo
            _filesController.HttpContext.Request.ContentLength = 10000000;

            // Act
            var result = await _filesController.UploadFile();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("La taille de la requête dépasse la limite", badRequestResult.Value?.ToString());
        }
                
        [Fact]
        public async Task UploadFile_ThrowsCustomDatashareException()
        {
            // Arrange
            // On configure le mock pour qu'il lance une CustomDatashareException
            _fileServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>()))
                            .ThrowsAsync(new CustomDatashareException(HttpStatusCode.BadRequest, "Type de fichier non autorisé."));

            // Act
            var result = await _filesController.UploadFile();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            // Vérification du message d'erreur retourné via ResolveCustomException
            var routeValues = badRequestResult.Value;
            var message = routeValues?.GetType().GetProperty("message")?.GetValue(routeValues, null);
            Assert.Equal("Type de fichier non autorisé.", message);
        }

        [Fact]
        public async Task UploadFile_ThrowsGenericException_ReturnsInternalServerError()
        {
            // Arrange
            // On configure le mock pour qu'il lance une Exception classique
            _fileServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>()))
                            .ThrowsAsync(new Exception("Une erreur d'écriture inattendue est survenue sur le disque."));

            // Act
            var result = await _filesController.UploadFile();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task DownloadFile_ValidToken_ReturnsFileStream()
        {
            // Arrange
            string token = "valid-token";
            var fakePasswordPayload = new DownloadFileRequest { Password = "SecretPassword" };

            var fakeStream = new MemoryStream();
            var expectedResponse = new DownloadFileResponse
            {
                Success = true,
                FileStream = fakeStream,
                ContentType = "application/pdf",
                FileName = "test.pdf"
            };

            _fileServiceMock.Setup(s => s.DownloadFileAsync(token, fakePasswordPayload.Password))
                            .ReturnsAsync(expectedResponse);

            // Act
            var result = await _filesController.DownloadFile(token, fakePasswordPayload);

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal("test.pdf", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task DownloadFile_ThrowsCustomDatashareException()
        {
            // Arrange
            string token = "any-token";
            var request = new DownloadFileRequest { Password = "WrongPassword" };

            // On configure le mock pour qu'il lance une CustomDatashareException
            _fileServiceMock.Setup(s => s.DownloadFileAsync(token, request.Password))
                            .ThrowsAsync(new CustomDatashareException(HttpStatusCode.BadRequest, "Mot de passe incorrect."));

            // Act
            var result = await _filesController.DownloadFile(token, request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);                        
        }

        [Fact]
        public async Task DownloadFile_ThrowsGenericException()
        {
            // Arrange
            string token = "any-token";
            var fakePasswordPayload = new DownloadFileRequest { Password = "SecretPassword" };

            // On configure le mock pour qu'il lance une Exception classique
            _fileServiceMock.Setup(s => s.DownloadFileAsync(token, fakePasswordPayload.Password))
                            .ThrowsAsync(new Exception("Une erreur serveur inattendue est survenue."));

            // Act
            var result = await _filesController.DownloadFile(token, fakePasswordPayload);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);                        
        }

        [Fact]
        public async Task GetFileMetaDataByToken_ValidToken_ReturnsOk()
        {
            // Arrange
            string token = "my-token";
            var metaData = new FileMetaData
            {
                OriginalName = "photo.png",
                Size = "1024",
                Extension = ".png",
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpirationDays = 7,
                Tags = ["Vacances"],
                UserId = 1
            };

            _fileServiceMock.Setup(s => s.GetFileMetaDataByTokenAsync(token))
                            .ReturnsAsync(metaData);

            // Act
            var result = await _filesController.GetFileMetaDataByToken(token);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Le retour doit être du type FileMetaDataResponse
            var response = Assert.IsType<FileMetaDataResponse>(okResult.Value);
            Assert.Equal("photo.png", response.OriginalFileName);
            Assert.Equal(".png", response.Extension);
            Assert.Equal(token, response.Token);
            // Vérifie que FileUtils a bien transformé les données
            Assert.False(response.IsExpired); 
        }

        [Fact]
        public async Task GetFileMetaDataByToken_ThrowsGenericException()
        {
            // Arrange
            string token = "any-token";

            // On configure le mock pour qu'il lance une Exception classique
            _fileServiceMock.Setup(s => s.GetFileMetaDataByTokenAsync(token))
                            .ThrowsAsync(new Exception("Une erreur serveur inattendue est survenue lors de la récupération des métadonnées."));

            // Act
            var result = await _filesController.GetFileMetaDataByToken(token);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetAllFiles_ThrowsGenericException()
        {
            // Arrange
            // On configure le mock pour qu'il lance une Exception
            _fileServiceMock.Setup(s => s.GetAllFileMetaDatasAsync(It.IsAny<int>()))
                            .ThrowsAsync(new Exception("Une erreur inattendue est survenue lors de la récupération des fichiers."));

            // Act
            var result = await _filesController.GetAllFiles();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);                        
        }

        [Fact]
        public async Task GetAllFiles_MultipleFilesExist_ReturnsOk()
        {
            // Arrange
            var metaDatas = new List<FileMetaData>
            {
                new FileMetaData { OriginalName = "file1.txt", Size = "100", Extension = ".txt", Token = "1", UserId = 1 },
                new FileMetaData { OriginalName = "file2.txt", Size = "200", Extension = ".txt", Token = "2", UserId = 1 }
            };

            _fileServiceMock.Setup(s => s.GetAllFileMetaDatasAsync(1)) // User 1 configuré dans SetupMockUserContext
                            .ReturnsAsync(metaDatas);

            // Act
            var result = await _filesController.GetAllFiles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var responsesList = Assert.IsType<List<FileMetaDataResponse>>(okResult.Value);
            Assert.Equal(2, responsesList.Count);
        }
                
        [Fact]
        public async Task DeleteFile_UserNotConnected_ReturnsUnauthorized()
        {
            // Arrange
            // On simule un utilisateur avec ID invalide (ou introuvable)
            SetupMockUserContext(0); 

            // Act
            var result = await _filesController.DeleteFile("some-token");

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);

            var routeValues = unauthorizedResult.Value;
            var message = routeValues?.GetType().GetProperty("message")?.GetValue(routeValues, null);
            Assert.Equal("Utilisateur non authentifié.", message);
        }

        [Fact]
        public async Task DeleteFile_ServiceReturnsFalse_ReturnsBadRequest()
        {
            // Arrange
            // Le service échoue à supprimer
            string token = "token123";
            _fileServiceMock.Setup(s => s.DeleteFileAsync(token, 1))
                            .ReturnsAsync(false);

            // Act
            var result = await _filesController.DeleteFile(token);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task DeleteFile_InvalidToken_Throws_CustomDatashareException()
        {
            // Arrange
            string token = "token123";
            _fileServiceMock.Setup(s => s.DeleteFileAsync(token, 1))
                            .ThrowsAsync(new CustomDatashareException(HttpStatusCode.NotFound, "Fichier introuvable."));

            // Act
            var result = await _filesController.DeleteFile(token);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, notFoundResult.StatusCode);
                      
        }

        [Fact]
        public async Task DeleteFile_ValidTokenAndUser_ReturnsOk()
        {
            // Arrange
            // L'utilisateur 1 est déjà configuré dans le constructeur
            _fileServiceMock.Setup(s => s.DeleteFileAsync("token123", 1))
                            .ReturnsAsync(true);

            // Act
            var result = await _filesController.DeleteFile("token123");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var routeValues = okResult.Value;
            var message = routeValues?.GetType().GetProperty("message")?.GetValue(routeValues, null);
            Assert.Equal("Fichier supprimé avec succès.", message);
        }

        [Fact]
        public async Task DeleteFile_ThrowsGenericException()
        {
            // Arrange
            string token = "token-error";
            _fileServiceMock.Setup(s => s.DeleteFileAsync(token, 1))
                            .ThrowsAsync(new Exception("Erreur système lors de la suppression."));

            // Act
            var result = await _filesController.DeleteFile(token);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task Action_ServiceThrowsException_ReturnsCorrectStatusCode()
        {
            // Test générique pour s'assurer que ResolveCustomException marche bien pour le controlleur
            // Arrange
            _fileServiceMock.Setup(s => s.GetFileMetaDataByTokenAsync("badtoken"))
                            .ThrowsAsync(new CustomDatashareException(HttpStatusCode.NotFound, "Document supprimé !"));

            // Act
            var result = await _filesController.GetFileMetaDataByToken("badtoken");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);

            var routeValues = notFoundResult.Value;
            var message = routeValues?.GetType().GetProperty("message")?.GetValue(routeValues, null);
            Assert.Equal("Document supprimé !", message);
        }
    }
}