using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Finance
{
    public class SalaryCalculationRequestDto
    {
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string SalaryType { get; set; } = string.Empty;
    }
}
