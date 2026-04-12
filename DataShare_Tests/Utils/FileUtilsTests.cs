using DataShare_API.Utils;
using System;
using Xunit;

namespace DataShare_Tests.Utils
{
    public class FileUtilsTests
    {
        [Fact]
        public void CalculateExpirationDetails_FileIsValid_ReturnsCorrectDetails()
        {
            // Arrange
            // Un fichier créé il y a 2 jours, qui expire dans 7 jours
            DateTime creationDate = DateTime.Now.AddDays(-2);
            int expirationDays = 7;

            // Act
            var details = FileUtils.CalculateExpirationDetails(creationDate, expirationDays);

            // Assert
            Assert.False(details.isExpired);
            Assert.Equal(creationDate.AddDays(7).Date, details.ExpirationDate.Date);
            Assert.Equal(5, details.RemainingDays);
        }

        [Fact]
        public void CalculateExpirationDetails_FileExpiresToday_ReturnsZeroRemainingDaysAndValid()
        {
            // Arrange
            // Un fichier créé il y a 3 jours, qui expire dans 3 jours (Aujourd'hui)
            // L'heure de création est dans le futur (ex: dans 2 heures) pour ne pas déclencher isExpired
            DateTime creationDate = DateTime.Now.AddDays(-3).AddHours(2);
            int expirationDays = 3;

            // Act
            var details = FileUtils.CalculateExpirationDetails(creationDate, expirationDays);

            // Assert
            Assert.False(details.isExpired);
            Assert.Equal(0, details.RemainingDays);
        }

        [Fact]
        public void CalculateExpirationDetails_FileExpiredToday_TimePassed_ReturnsExpired()
        {
            // Arrange
            // Un fichier créé il y a 3 jours pleins, MAIS l'heure d'expiration (création + 3j) 
            // est passée de 2 heures par rapport à l'heure actuelle.
            DateTime creationDate = DateTime.Now.AddDays(-3).AddHours(-2);
            int expirationDays = 3;

            // Act
            var details = FileUtils.CalculateExpirationDetails(creationDate, expirationDays);

            // Assert
            Assert.True(details.isExpired);
            Assert.Equal(0, details.RemainingDays);
        }

        [Fact]
        public void CalculateExpirationDetails_FileIsExpired_ReturnsExpiredAndZeroRemainingDays()
        {
            // Arrange
            // Un fichier créé il y a 10 jours, qui devait expirer au bout de 5 jours
            DateTime creationDate = DateTime.Now.AddDays(-10);
            int expirationDays = 5;

            // Act
            var details = FileUtils.CalculateExpirationDetails(creationDate, expirationDays);

            // Assert
            Assert.True(details.isExpired);
            Assert.Equal(0, details.RemainingDays);
            Assert.Equal(creationDate.AddDays(5).Date, details.ExpirationDate.Date);
        }
                
        [Theory]
        [InlineData(500, "500,00 o")]
        [InlineData(1024, "1,00 Ko")]
        [InlineData(1048576, "1,00 Mo")]
        public void FormatFileSize_VariousSizes_ReturnsCorrectString(long bytes, string expected)
        {
            // Act
            var result = FileUtils.FormatFileSize(bytes);

            // Assert
            // Remarque : Selon la culture (fr-FR ou en-US) du système hébergeant le test, 
            // la virgule pourrait être un point "1.00 Ko". Replace change temporairement cela si besoin.
            Assert.Equal(expected.Replace(".", ","), result.Replace(".", ","));
        }
    }
}