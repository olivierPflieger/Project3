using System.ComponentModel.DataAnnotations;

namespace DataShare_API.DTO
{
    /// <summary>
    /// Modèle de requête pour la création d'un nouvel utilisateur.
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// L'adresse e-mail de l'utilisateur.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "Email invalide")] 
        public required string Email { get; set; }

        /// <summary>
        /// Le mot de passe de l'utilisateur (doit contenir au moins 8 caractères).
        /// </summary>
        [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")] 
        public required string Password { get; set; }
    }
}
