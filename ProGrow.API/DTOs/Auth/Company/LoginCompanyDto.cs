using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Auth.Company
{
    public class LoginCompanyDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
