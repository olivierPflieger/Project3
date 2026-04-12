using Amazon.S3;
using Amazon.S3.Model;
using DataShare_API.Exceptions;
using DataShare_API.Models;
using DataShare_API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;

namespace DataShare_API.Tests.Services
{
    public class FileServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IAmazonS3> _s3ClientMock;
        private readonly Mock<IOptions<FileUploadSettings>> _settingsMock;
        private readonly FileService _fileService;
        private readonly FileMetaData _testFileMetaData;
        private readonly FileMetaData _testFileMetaData2;

        public FileServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            _s3ClientMock = new Mock<IAmazonS3>();
            _s3ClientMock.SetupGet(x => x.Config).Returns(new AmazonS3Config());

            var settings = new FileUploadSettings { AwsBucketName = "test-bucket" };
            _settingsMock = new Mock<IOptions<FileUploadSettings>>();
            _settingsMock.Setup(s => s.Value).Returns(settings);

            _fileService = new FileService(_settingsMock.Object, _s3ClientMock.Object, _context);

            _testFileMetaData = new FileMetaData
            {
                Id = 1,
                OriginalName = "test.txt",
                Token = Guid.NewGuid().ToString(),
                Extension = ".txt",
                Size = "1024",
                Password = null,
                CreatedAt = DateTime.UtcNow,
                ExpirationDays = 7,
                Tags = new[] { "tag1", "tag2" },
                UserId = 1
            };

            _testFileMetaData2 = new FileMetaData
            {
                Id = 2,
                OriginalName = "test2.txt",
                Token = Guid.NewGuid().ToString(),
                Extension = ".txt",
                Size = "2048",
                Password = null,
                CreatedAt = DateTime.UtcNow,
                ExpirationDays = 5,
                Tags = new[] { "tag3", "tag4" },
                UserId = 2
            };
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
                
        [Fact]
        public async Task UploadFileAsync_InvalidContentType_Throws_CustomDatashareException()
        {
            // Arrange
            using var stream = new MemoryStream();
            var invalidContentType = "application/json";

            // Act & Assert            
            var ex = await Assert.ThrowsAsync<CustomDatashareException>(() =>
                _fileService.UploadFileAsync(stream, invalidContentType, userId: 1));

            Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
            Assert.Equal("Le type de contenu attendu est 'multipart/form-data'.", ex.Message);
        }

        [Fact]
        public async Task UploadFileAsync_EmptyMultipart_Throws_CustomDatashareException()
        {
            // Arrange
            var boundary = "----TestBoundary123";
            var contentType = $"multipart/form-data; boundary={boundary}";

            var body = $"--{boundary}\r\n" +
                       $"Content-Disposition: form-data; name=\"dummy\"\r\n\r\n" +
                       $"dummy-value\r\n" +
                       $"--{boundary}--\r\n";

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(body));

            // Act and Assert
            var ex = await Assert.ThrowsAsync<CustomDatashareException>(() =>
                _fileService.UploadFileAsync(stream, contentType, userId: 1));

            Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
            Assert.Equal("Aucun fichier valide n'a été trouvé", ex.Message);
        }

        [Fact]
        public async Task UploadFileAsync_InvalidExtension_Throws_CustomDatashareException()
        {
            // Arrange
            // On simule l'envoi d'un fichier .exe
            var (stream, contentType) = await CreateTrueMultipartStreamAsync("virus.exe", "fake content", "application/x-msdownload");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<CustomDatashareException>(() => 
            _fileService.UploadFileAsync(stream, contentType, userId: 1));

            Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
            Assert.Equal("Type de fichier non autorisé", ex.Message);
            stream.Dispose();
        }

        [Fact]
        public async Task UploadFileAsync_MimeTypeMismatch_Throws_CustomDatashareException()
        {
            // Arrange
            // On déclare une extension .txt, mais on envoie un MIME type d'image
            var (stream, contentType) = await CreateTrueMultipartStreamAsync("document.txt", "texte", "image/png");

            // Act & Assert
            // Act & Assert
            var ex = await Assert.ThrowsAsync<CustomDatashareException>(() =>
                _fileService.UploadFileAsync(stream, contentType, userId: 1));

            Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
            Assert.Contains("Le type de contenu (MIME) ne correspond pas", ex.Message);
            stream.Dispose();
        }

        [Fact]
        public async Task UploadFileAsync_InvalidSignatureMagicNumber_Throws_CustomDatashareException()
        {
            // Arrange
            var maliciousContent = "Ceci n'est pas un PNG, c'est juste du texte ou du code malveillant.";
            var (stream, contentType) = await CreateTrueMultipartStreamAsync("fausse-image.png", maliciousContent, "image/png");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<CustomDatashareException>(() =>
                _fileService.UploadFileAsync(stream, contentType, userId: 1));

            Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
            Assert.Equal("Signature de fichier non autorisée", ex.Message);

            stream.Dispose();
        }

        [Fact]
        public async Task UploadFileAsync_ValidFile_ReturnsSuccessAndSavesToDatabase()
        {
            // Arrange
            var fileName = "document.pdf";
            var fileContent = "%PDF-1.4\nCeci est le reste du contenu";
                        
            var (stream, contentType) = await CreateTrueMultipartStreamAsync(fileName, fileContent, "application/pdf");

            _s3ClientMock.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new PutObjectResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

            _s3ClientMock.Setup(s => s.InitiateMultipartUploadAsync(It.IsAny<InitiateMultipartUploadRequest>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new InitiateMultipartUploadResponse { UploadId = "123" });

            _s3ClientMock.Setup(s => s.GetObjectMetadataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new GetObjectMetadataResponse
                         {
                             ContentLength = fileContent.Length
                         });

            // Act
            var response = await _fileService.UploadFileAsync(stream, contentType, userId: 1);

            // Assert
            Assert.Equal(fileName, response.OriginalFileName);
            Assert.Equal(".pdf", response.Extension);
            Assert.NotNull(response.Token);

            var fileInDb = await _context.FileMetaDatas.FirstOrDefaultAsync(f => f.Token == response.Token);
            Assert.NotNull(fileInDb);
            Assert.Equal(1, fileInDb.UserId);
        }

        [Fact]
        public async Task DownloadFileAsync_TokenNotFound_Throws_CustomDatashareException()
        {
            // Act
            var ex = await Assert.ThrowsAsync<CustomDatashareException>(() =>
                    _fileService.DownloadFileAsync("invalid-token", null));

            Assert.Equal(HttpStatusCode.NotFound, ex.Code);
            Assert.Contains("Fichier introuvable ou token invalide", ex.Message);
        }

        [Fact]
        public async Task DownloadFileAsync_FileIsProtected_NoPasswordProvided_Throws_CustomDatashareException()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();
            var protectedFile = new FileMetaData
            {
                Id = 3,
                Token = token,
                OriginalName = "secret.txt",
                Extension = ".txt",
                Size = "1024",
                Password = BCrypt.Net.BCrypt.HashPassword("MonMotDePasseSecret"),
                UserId = 1
            };

            _context.FileMetaDatas.Add(protectedFile);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<CustomDatashareException>(() =>
                _fileService.DownloadFileAsync(token, null)); // password null ou vide

            Assert.Equal(HttpStatusCode.Conflict, ex.Code);
            Assert.Equal("Ce fichier est protégé par un mot de passe", ex.Message);            
        }

        [Fact]
        public async Task DownloadFileAsync_FileIsProtected_WrongPassword_Throws_CustomDatashareException()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();
            var protectedFile = new FileMetaData
            {
                Id = 4,
                Token = token,
                OriginalName = "secret.txt",
                Extension = ".txt",
                Size = "1024",
                Password = BCrypt.Net.BCrypt.HashPassword("MonMotDePasseSecret"), // Fichiers protégé
                UserId = 1
            };

            _context.FileMetaDatas.Add(protectedFile);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<CustomDatashareException>(() =>
                _fileService.DownloadFileAsync(token, "MauvaisMotDePasse"));

            Assert.Equal(HttpStatusCode.Conflict, ex.Code);
            Assert.Equal("Mot de passe incorrect", ex.Message);
        }

        [Fact]
        public async Task DownloadFileAsync_ValidFile_ReturnsStream()
        {
            // Arrange
            _context.FileMetaDatas.Add(_testFileMetaData);
            await _context.SaveChangesAsync();

            var expectedStream = new MemoryStream();
            _s3ClientMock.Setup(s3 => s3.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new GetObjectResponse { ResponseStream = expectedStream, Headers = { ContentType = "text/plain" } });

            // Act
            var response = await _fileService.DownloadFileAsync(_testFileMetaData.Token, null);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(expectedStream, response.FileStream);
            Assert.Equal(_testFileMetaData.OriginalName, response.FileName);
        }

        [Fact]
        public async Task Get_FileMetaData_ByToken_Returns_CorrectObject()
        {
            // Arrange
            _context.FileMetaDatas.Add(_testFileMetaData);
            await _context.SaveChangesAsync();

            // Act
            var result = await _fileService.GetFileMetaDataByTokenAsync(_testFileMetaData.Token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testFileMetaData.Id, result.Id);
            Assert.Equal(_testFileMetaData.Extension, result.Extension);
            Assert.Equal(_testFileMetaData.Size, result.Size);
            Assert.Equal(_testFileMetaData.Password, result.Password);
            Assert.Equal(_testFileMetaData.CreatedAt, result.CreatedAt);
            Assert.Equal(_testFileMetaData.ExpirationDays, result.ExpirationDays);
            Assert.Equal(_testFileMetaData.Tags, result.Tags);
            Assert.Equal(_testFileMetaData.Token, result.Token);
            Assert.Equal(_testFileMetaData.OriginalName, result.OriginalName);
            Assert.Equal(_testFileMetaData.UserId, result.UserId);
        }

        [Fact]
        public async Task Get_FileMetaData_ByWrongToken_Throws_CustomDatashareException()
        {
            // Arrange
            _context.FileMetaDatas.Add(_testFileMetaData);
            await _context.SaveChangesAsync();
            string wrongToken = _testFileMetaData.Token + "dummy";
                        
            // Assert
            var ex = await Assert.ThrowsAsync<CustomDatashareException>(() => 
                _fileService.GetFileMetaDataByTokenAsync(wrongToken));

            Assert.Equal(HttpStatusCode.NotFound, ex.Code);
            Assert.Equal("Fichier introuvable", ex.Message);
        }

        [Fact]
        public async Task Get_All_FileMetaDatas_ReturnsUserFilesOnly()
        {
            // Arrange
            _context.FileMetaDatas.AddRange(_testFileMetaData, _testFileMetaData2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _fileService.GetAllFileMetaDatasAsync(userId: 1);

            // Assert
            Assert.Single(result);
            Assert.All(result, f => Assert.Equal(1, f.UserId));
            Assert.Equal(_testFileMetaData.Token, result.First().Token);
        }

        [Fact]
        public async Task Delete_File_UserNotOwner_Throws_CustomDatashareException()
        {
            // Arrange
            _context.FileMetaDatas.Add(_testFileMetaData);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<CustomDatashareException>(() => 
            _fileService.DeleteFileAsync(_testFileMetaData.Token, userId: 9));

            Assert.Equal(HttpStatusCode.NotFound, ex.Code);
            Assert.Equal("Suppression impossible, ce fichier n'existe pas ou ne vous appartient pas", ex.Message);
        }

        [Fact]
        public async Task DeleteFile_UserIsOwner_DeletesFromS3AndDatabase()
        {
            // Arrange
            _context.FileMetaDatas.Add(_testFileMetaData);
            await _context.SaveChangesAsync();

            _s3ClientMock.Setup(s3 => s3.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new DeleteObjectResponse());

            // Act
            var result = await _fileService.DeleteFileAsync(_testFileMetaData.Token, userId: 1);

            // Assert
            Assert.True(result);
            Assert.Empty(_context.FileMetaDatas); // Checks file was deleted from DB
            _s3ClientMock.Verify(s3 => s3.DeleteObjectAsync(It.Is<DeleteObjectRequest>(r => r.Key == $"{_testFileMetaData.Token}{_testFileMetaData.Extension}"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteFile_S3_ThrowsException()
        {
            // Arrange
            _context.FileMetaDatas.Add(_testFileMetaData);
            await _context.SaveChangesAsync();

            var awsSecretMessage = "Access Denied";
            _s3ClientMock.Setup(s3 => s3.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new AmazonS3Exception(awsSecretMessage));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => 
            _fileService.DeleteFileAsync(_testFileMetaData.Token, userId: 1));

            Assert.Contains($"Erreur lors de la suppression du fichier sur AWS S3 : {awsSecretMessage}", ex.Message);

            // Vérifier que le fichier n'a PAS été supprimé de la base de données (car la suppression S3 a échoué)
            Assert.Single(_context.FileMetaDatas);
        }

        /// <summary>
        /// Génère un flux HTTP Multipart
        /// </summary>
        private async Task<(MemoryStream stream, string contentType)> CreateTrueMultipartStreamAsync(string fileName, string content, string fileContentType)
        {
            var multipartContent = new MultipartFormDataContent();
                        
            var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(content));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(fileContentType);
            multipartContent.Add(fileContent, "file", fileName);

            var memoryStream = new MemoryStream();
            await multipartContent.CopyToAsync(memoryStream);
                        
            memoryStream.Position = 0;
                        
            var contentType = multipartContent.Headers.ContentType?.ToString() ?? "multipart/form-data";

            return (memoryStream, contentType);
        }
    }
}