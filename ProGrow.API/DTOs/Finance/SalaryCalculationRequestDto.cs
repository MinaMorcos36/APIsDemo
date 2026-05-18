namespace ProGrow.API.DTOs.Finance
{
    public class SalaryCalculationRequestDto
    {
        public decimal Amount { get; set; }
        public string SalaryType { get; set; } = string.Empty;
    }
}
