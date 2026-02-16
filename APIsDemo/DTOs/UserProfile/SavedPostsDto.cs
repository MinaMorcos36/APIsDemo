namespace APIsDemo.DTOs.UserProfile
{
    public class SavedPostsDto
    {
        public int SavedPostId { get; set; }
        public DateTime? SavedAt { get; set; }

        public int PostId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public int AuthorId { get; set; }
        public string? AuthorType { get; set; }
    }
}
