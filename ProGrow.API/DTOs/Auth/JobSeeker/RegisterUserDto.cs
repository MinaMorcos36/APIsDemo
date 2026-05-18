using ProGrow.API.Models;
using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Auth.JobSeeker
{
    public class RegisterUserDto
    {
        [Required]
        [StringLength(60)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(60)]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        [Required]
        public DateOnly Birthdate { get; set; }
        [Required]
        [Phone]
        public string Phone { get; set; }
    }
}
