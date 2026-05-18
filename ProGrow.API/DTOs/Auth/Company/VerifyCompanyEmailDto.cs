using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Auth.Company
{
    public class VerifyCompanyEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; }
    }
}
