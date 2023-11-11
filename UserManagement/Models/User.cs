using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models
{
    [Table("user", Schema = "dbo")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [MinLength(3, ErrorMessage = "The name must contain at least 2 characters")]
        [MaxLength(30, ErrorMessage = "The name must contain a maximum of 30 characters")]
        public string Name { get; set; }

        [Column("email_address")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string EmailAddress { get; set; }

        [Column("role")]
        public string Role { get; set; }

        [MinLength(5, ErrorMessage = "Password must contain at least 5 characters")]
        [Column("password")]
        public string Password { get; set; }

        public byte[] PasswordSalt { get; set; } = new byte[32];

        public int? VerificationToken { get; set; }

        public bool IsVerified { get; set; } = false;

        public int? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpires { get; set; } 
    }
}
