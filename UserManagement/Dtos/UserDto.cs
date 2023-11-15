using System.ComponentModel.DataAnnotations;

namespace UserManagement.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }

        [MinLength(3, ErrorMessage = "The name must contain at least 2 characters")]
        [MaxLength(30, ErrorMessage = "The name must contain a maximum of 30 characters")]
        public string Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string EmailAddress { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
