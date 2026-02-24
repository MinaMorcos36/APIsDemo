using APIsDemo.Models;
using System.ComponentModel.DataAnnotations;

namespace APIsDemo.DTOs.Auth.JobSeeker
{
    public class RegisterUserDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
        [Required]
        public DateOnly Birthdate { get; set; }
        [Required]
        public string Phone { get; set; }
    }
}
