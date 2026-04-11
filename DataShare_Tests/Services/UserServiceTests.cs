using Microsoft.EntityFrameworkCore;
using DataShare_API.DTO;
using DataShare_API.Exceptions;
using DataShare_API.Models;
using DataShare_API.Services;

namespace DataShare_API.Tests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // Configuration de la base de données en mémoire,
            // avec un GUID unique pour s'assurer que chaque instance de test ait une base vierge
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            // Création du service à tester avec l'injection du vrai DBContext (configuré en InMemory)
            _userService = new UserService(_context);
        }

        public void Dispose()
        {
            // Nettoyage après chaque test
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateUserAsync_ValidRequest_CreatesUserAndReturnsSuccess()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            var result = await _userService.CreateUserAsync(request);

            // Assert            
            Assert.True(result == 1); 
         
            // On vérifie que l'utilisateur a bien été créé en base
            var userInDb = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
            Assert.NotNull(userInDb);
            Assert.Equal(request.Email, userInDb.Email);

            // On vérifie que le mot de passe N'EST PAS en clair et qu'il est bien haché
            Assert.NotEqual(request.Password, userInDb.Password);
            Assert.True(BCrypt.Net.BCrypt.Verify(request.Password, userInDb.Password));
        }

        [Fact]
        public async Task CreateUserAsync_ExistingEmail_ThrowsException()
        {
            // Arrange            
            var existingUser = new User
            {
                Email = "existing@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("OldPassword123!")
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();
                        
            var request = new CreateUserRequest
            {
                Email = "existing@example.com",
                Password = "NewPassword123!"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<CustomDatashareException>(() => 
                _userService.CreateUserAsync(request));

            Assert.Equal("Un utilisateur existe déjà avec cet email", ex.Message);            
        }

        [Fact]
        public async Task FindById_UserExists_ReturnsUserObject()
        {
            // Arrange
            var user = new User
            {
                Email = "findme@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123!")
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act            
            var result = await _userService.FindById(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task FindById_UserDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _userService.FindById(999); // ID inexistant en base

            // Assert
            Assert.Null(result);
        }
    }
}