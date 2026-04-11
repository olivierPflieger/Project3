using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using DataShare_API.Models;
using DataShare_API.Services;

namespace DataShare_API.Tests.Services
{
    public class LoginServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IConfigurationSection> _jwtSectionMock;
        private readonly LoginService _loginService;

        public LoginServiceTests()
        {
            // 1. Initialiser la BD en mémoire (comme pour FileService et UserService)
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            // 2. Mocker IConfiguration pour la génération du JWT
            _configurationMock = new Mock<IConfiguration>();
            _jwtSectionMock = new Mock<IConfigurationSection>();

            // On configure la section "Jwt" demandée par GetSection()     
            // Il faut au moins 16 caractères pour que HMAC SHA256 ne plante pas au runtime
            var testSecretKey = "SuperSecr3tKey123!456VeryLongString";

            _jwtSectionMock.Setup(s => s["Key"]).Returns(testSecretKey);
            _jwtSectionMock.Setup(s => s["Issuer"]).Returns("MonIssuerTest");
            _jwtSectionMock.Setup(s => s["Audience"]).Returns("MonAudienceTest");

            _configurationMock.Setup(c => c.GetSection("Jwt")).Returns(_jwtSectionMock.Object);

            // 3. Injecter l'ensemble dans LoginService
            _loginService = new LoginService(_context, _configurationMock.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsJwtToken()
        {
            // Arrange
            string validPassword = "MySecurePassword!";
            var user = new User
            {
                Email = "valid@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword(validPassword) // On stocke bien le hash
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            var token = _loginService.Login("valid@example.com", validPassword);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);

            // Optionnel: vérifier que le token est bien un vrai JWT string (3 parties séparées par des points)
            var parts = token.Split('.');
            Assert.Equal(3, parts.Length);
        }

        [Fact]
        public void Login_UnknownEmail_ReturnsNull()
        {
            // Arrange
            // La BD est vide

            // Act
            var token = _loginService.Login("unknown@example.com", "SomePassword!");

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public void Login_WrongPassword_ReturnsNull()
        {
            // Arrange
            var user = new User
            {
                Email = "wrongpass@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("MySecurePassword!")
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            var token = _loginService.Login("wrongpass@example.com", "MauvaisMotDePasse");

            // Assert
            Assert.Null(token);
        }                
    }
}