using System;
using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Community.Jobs
{
    public class JobResponseDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; } = null!;
        public string ShortDescription { get; set; } = null!;
        public string LocationMode { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public required string JobType { get; set; }
        public required string CityOffice { get; set; }
        public int JobCategoryId { get; set; }
        public string JobCategoryName { get; set; } = string.Empty;
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public required bool IsSalaryInInterview { get; set; } = false;
        public required string BannerImageUrl { get; set; }
        public required string AboutRole { get; set; }
        public required string Responsibilities { get; set; }
        public required string Requirements { get; set; }
        public required List<string> RequiredSkills { get; set; }
    }
}
