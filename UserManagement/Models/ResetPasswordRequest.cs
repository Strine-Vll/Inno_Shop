using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models
{
    public class ResetPasswordRequest
    {
        [Required]
        public int ResetToken { get; set; }

        [MinLength(5, ErrorMessage = "Password must contain at least 5 characters")]
        [Column("password")]
        public string Password { get; set; }

        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
