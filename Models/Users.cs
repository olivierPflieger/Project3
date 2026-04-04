using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Project3.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {        
        public int Id { get; set; }
                        
        [Required]        
        [EmailAddress(ErrorMessage = "Email invalide")]
        public required string Email { get; set; }
        
        [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
        public required string Password { get; set; }
    }
}
