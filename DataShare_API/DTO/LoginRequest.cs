using System.ComponentModel.DataAnnotations;

namespace DataShare_API.DTO
{
    /// <summary>
    /// Modèle de requête pour l'authentification d'un utilisateur.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Adresse e-mail enregistrée de l'utilisateur
        /// </summary>
        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Le format de l'email est invalide.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mot de passe de l'utilisateur (en clair pour l'envoi, sera vérifié de manière sécurisée).
        /// </summary>
        [Required(ErrorMessage = "Le mot de passe est requis.")]
        public string Password { get; set; } = string.Empty;
    }
}