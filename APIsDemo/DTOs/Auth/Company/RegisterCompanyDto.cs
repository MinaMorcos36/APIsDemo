using System.ComponentModel.DataAnnotations;

namespace APIsDemo.DTOs.Auth.Company
{
    public class RegisterCompanyDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

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
        public int IndustryId { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? WebsiteUrl { get; set; }

        public string? PictureUrl { get; set; }
    }
}
