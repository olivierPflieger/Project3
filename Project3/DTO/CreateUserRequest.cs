using System.ComponentModel.DataAnnotations;

namespace Project3.DTO
{
    public class CreateUserRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Email invalide")] 
        public required string Email { get; set; }

        [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")] 
        public required string Password { get; set; }
    }
}
