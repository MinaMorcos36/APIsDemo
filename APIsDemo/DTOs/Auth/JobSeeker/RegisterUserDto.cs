using System.ComponentModel.DataAnnotations;

namespace APIsDemo.DTOs.Auth.JobSeeker
{
    public class RegisterUserDto
    {
        [Required]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
