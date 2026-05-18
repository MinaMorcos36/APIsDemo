using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Auth.JobSeeker
{
    public class VerifyUserEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; }
    }
}
