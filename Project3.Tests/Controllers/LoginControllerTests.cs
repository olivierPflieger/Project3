using Microsoft.AspNetCore.Mvc;
using Moq;
using Project3.Controllers;
using Project3.DTO;
using Project3.Interfaces;

namespace Project3.Tests.Controllers
{
    public class LoginControllerTests
    {
        private readonly Mock<ILoginService> _loginServiceMock;
        private readonly LoginController _loginController;

        public LoginControllerTests()
        {
            // Mock LoginService
            _loginServiceMock = new Mock<ILoginService>();
            _loginController = new LoginController(_loginServiceMock.Object);
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var validRequest = new LoginRequest
            {
                Email = "user@example.com",
                Password = "Password123!"
            };

            var fakeGeneratedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.FakeTokenPayload.FakeSignature";

            // On simule une connexion réussie depuis le service, qui retourne le Token string
            _loginServiceMock.Setup(s => s.Login("user@example.com", "Password123!"))
                             .Returns(fakeGeneratedToken);

            // Act
            var result = _loginController.Login(validRequest);

            // Assert
            // Vérifie qu'on a bien reçu un code 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // On vérifie que la valeur est de type anonyme { token = "..." } et qu'elle contient le bon token
            var routeValues = okResult.Value;
            var returnedToken = routeValues?.GetType().GetProperty("token")?.GetValue(routeValues, null);

            Assert.Equal(fakeGeneratedToken, returnedToken);
        }

        [Fact]
        public void Login_InvalidCredentials_Returns_UnauthorizedWithMessage()
        {
            // Arrange
            var invalidRequest = new LoginRequest
            {
                Email = "user@example.com",
                Password = "WrongPassword!"
            };

            // On simule un echec (mot de passe incorrect ou email introuvable renvoie "null" dans votre service)
            _loginServiceMock.Setup(s => s.Login(It.IsAny<string>(), It.IsAny<string>()))
                             .Returns((string?)null);

            // Act
            var result = _loginController.Login(invalidRequest);

            // Assert
            // Doit retourner 401 Unauthorized
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);

            // Vérifie que le message "Email ou mot de passe incorrect" est bien présent
            var routeValues = unauthorizedResult.Value;
            var returnedMessage = routeValues?.GetType().GetProperty("message")?.GetValue(routeValues, null);

            Assert.Equal("Email ou mot de passe incorrect.", returnedMessage);
        }

        [Fact]
        public void Login_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "error@example.com",
                Password = "Password123!"
            };

            var unexpectedExceptionMessage = "Connexion indisponible (Exception mockée)";

            // On simule une erreur grave classique
            _loginServiceMock.Setup(s => s.Login(It.IsAny<string>(), It.IsAny<string>()))
                             .Throws(new Exception(unexpectedExceptionMessage));

            // Act
            var result = _loginController.Login(request);

            // Assert
            // Le contrôleur utilise StatusCode(500, ...)
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            // Vérifie la concacténation avec le message d'erreur
            var routeValues = statusCodeResult.Value;
            var returnedMessage = routeValues?.GetType().GetProperty("message")?.GetValue(routeValues, null);

            Assert.Equal($"Une erreur serveur s'est produite: {unexpectedExceptionMessage}", returnedMessage);
        }
    }
}