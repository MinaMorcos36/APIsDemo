using System.ComponentModel.DataAnnotations;

namespace APIsDemo.DTOs.UserProfile
{
    public class UpdateProfileDto
    {
        [MaxLength(500)]
        public string? Bio { get; set; }

        [MaxLength(100)]
        public string? Headline { get; set; }

        [MaxLength(100)]
        public string? Major { get; set; }

        [MaxLength(100)]
        public string? University { get; set; }

        public string? PictureUrl { get; set; }
        public string? CvUrl { get; set; }

        [MaxLength(60)]
        public string? FirstName { get; set; }

        [MaxLength(60)]
        public string? LastName { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        public DateOnly? Birthdate { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }
    }
}
