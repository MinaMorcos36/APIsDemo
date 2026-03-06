using System;

namespace APIsDemo.DTOs.Community.Jobs
{
    public class JobResponseDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}