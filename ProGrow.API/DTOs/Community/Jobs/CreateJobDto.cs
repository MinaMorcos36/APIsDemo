using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Community.Jobs
{
    public class CreateJobDto
    {
        [Required]
        [StringLength(150, MinimumLength = 1)]
        [RegularExpression(@".*\S.*")]
        public required string Title { get; set; }

        [Required]
        [StringLength(100)]
        public required string ShortDescription { get; set; }

        [Required]
        public required int LocationModeId { get; set; }

        [Required]
        public required int JobTypeId { get; set; }

        [Required]
        [StringLength(150)]
        public required string CityOffice { get; set; }

        [Required]
        public int JobCategoryId { get; set; }

        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }

        [Required]
        public required bool IsSalaryInInterview { get; set; } = false;


        [Required]
        [StringLength(2000)]
        public required string AboutRole { get; set; }

        [Required]
        [StringLength(2000)]
        public required string Responsibilities { get; set; }

        [Required]
        [StringLength(2000)]
        public required string Requirements { get; set; }

        public List<int> RequiredSkillIds { get; set; } = new();
    }
}
