using Microsoft.AspNetCore.Mvc;
using Moq;
using DataShare_API.Controllers;
using DataShare_API.DTO;
using DataShare_API.Exceptions;
using DataShare_API.Services;
using System.Net;

namespace DataShare_API.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UsersController _usersController;

        public UsersControllerTests()
        {
            // On a uniquement besoin de mocker IUserService pour le contrôleur
            _userServiceMock = new Mock<IUserService>();

            // On injecte le faux service dans le vrai contrôleur
            _usersController = new UsersController(_userServiceMock.Object);
        }

        [Fact]
        public async Task CreateUser_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var validRequest = new CreateUserRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Mock UserService
            _userServiceMock.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>()))
                            .ReturnsAsync(1);

            // Act
            var result = await _usersController.CreateUser(validRequest);

            // Assert
            // On vérifie que la réponse est un HTTP 201 Created
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);

            var routeValues = createdResult.Value;
            var messageProperty = routeValues?.GetType().GetProperty("message")?.GetValue(routeValues, null);
            Assert.Equal("Utilisateur correctement ajouté", messageProperty);
        }

        [Fact]
        public async Task CreateUser_ServiceThrowsExceptio0n_When_User_Already_Exists()
        {
            // Arrange
            var conflictRequest = new CreateUserRequest
            {
                Email = "existing@example.com",
                Password = "Password123!"
            };

            var exceptionMessage = "Un utilisateur existe déjà avec cet email";

            // On simule que le service remonte notre exception personnalisée avec un HttpStatusCode.Conflict (409)
            _userServiceMock.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>()))
                            .ThrowsAsync(new CustomDatashareException(HttpStatusCode.Conflict, exceptionMessage));

            // Act
            var result = await _usersController.CreateUser(conflictRequest);

            // Assert
            // ResolveCustomException doit avoir transformé ça en ConflictObjectResult
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);

            var routeValues = conflictResult.Value;
            var returnedMessage = routeValues?.GetType().GetProperty("message")?.GetValue(routeValues, null);
            Assert.Equal(exceptionMessage, returnedMessage);
        }

        [Fact]
        public async Task CreateUser_ServiceThrowsExceptio0n_When_Bad_Request()
        {
            // Arrange
            var badRequest = new CreateUserRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var exceptionMessage = "Données invalides";

            // On simule que le service remonte une BadRequest
            _userServiceMock.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>()))
                            .ThrowsAsync(new CustomDatashareException(HttpStatusCode.BadRequest, exceptionMessage));

            // Act
            var result = await _usersController.CreateUser(badRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var routeValues = badRequestResult.Value;
            var returnedMessage = routeValues?.GetType().GetProperty("message")?.GetValue(routeValues, null);
            Assert.Equal(exceptionMessage, returnedMessage);
        }

        [Fact]
        public async Task CreateUser_ServiceThrowsExceptio0n_When_Internal_Server_Error()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Email = "error@example.com",
                Password = "Password123!"
            };

            var unexpectedExceptionMessage = "Base de données déconnectée";

            // On simule une erreur grave classique (non maîtrisée)
            _userServiceMock.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>()))
                            .ThrowsAsync(new Exception(unexpectedExceptionMessage));

            // Act
            var result = await _usersController.CreateUser(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            // Le contrôleur est censé préfixer l'erreur de "Une erreur serveur s'est produite: "
            var routeValues = statusCodeResult.Value;
            var returnedMessage = routeValues?.GetType().GetProperty("message")?.GetValue(routeValues, null);
            Assert.Equal($"Une erreur serveur s'est produite: {unexpectedExceptionMessage}", returnedMessage);
        }
    }
}