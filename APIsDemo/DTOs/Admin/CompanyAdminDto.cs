namespace APIsDemo.DTOs.Admin
{
    public class CompanyAdminDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? WebsiteUrl { get; set; }
    }
}
