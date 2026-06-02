namespace ProGrow.API.DTOs.Community.Jobs
{
    public class JobFeedDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string ShortDescription { get; set; } = null!;
        public string LocationMode { get; set; } = null!;
        public string JobType { get; set; } = null!;
        public string CityOffice { get; set; } = null!;
        public int JobCategoryId { get; set; }
        public string JobCategoryName { get; set; } = string.Empty;
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public bool IsSalaryInInterview { get; set; }
        public string BannerImageUrl { get; set; } = null!;
        public string AboutRole { get; set; } = null!;
        public string Responsibilities { get; set; } = null!;
        public string Requirements { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? CompanyPictureUrl { get; set; }

        public int LikesCount { get; set; }
        public int SavesCount { get; set; }
        public bool IsLikedByMe { get; set; }
        public bool IsSavedByMe { get; set; }

        public int ApplicantsCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsAppliedByMe { get; set; }
        public bool IsActive { get; set; }
    }
}
