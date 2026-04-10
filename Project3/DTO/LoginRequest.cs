using System.ComponentModel.DataAnnotations;

namespace Project3.DTO
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Le format de l'email est invalide.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        public string Password { get; set; } = string.Empty;
    }
}