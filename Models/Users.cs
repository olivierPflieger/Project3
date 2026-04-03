using System.ComponentModel.DataAnnotations;

namespace Project3.Models
{
    public class User
    {        
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public required string FirstName { get; set; }
        
        [Required]
        [MaxLength(255)]
        public required string LastName { get; set; }
        
        [Required]        
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
