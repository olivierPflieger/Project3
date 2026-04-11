using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataShare_API.Models
{
    public class FileMetaData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string OriginalName { get; set; }
        
        [Required]
        public required string Token { get; set; }

        [Required]
        public required string Extension { get; set; }

        [Required]
        public required string Size { get; set; }

        // Warning : Angular doit renvoyer NULL (si non renseigné)
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        public string? Password { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ExpirationDays { get; set; } = 7;

        public string[] Tags { get; set; } = Array.Empty<string>();

        // User foreign key
        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}
