namespace APIsDemo.DTOs.UserProfile
{
    public class ProfileResponseDto
    {
        public string? Bio { get; set; }
        public string? Headline { get; set; }
        public string? Major { get; set; }
        public string? University { get; set; }
        public string? PictureUrl { get; set; }
        public string? CvUrl { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public DateOnly? Birthdate { get; set; }
        public string? Address { get; set; }
    }
}
