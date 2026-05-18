namespace ProGrow.API.DTOs.Finance
{
    public class SalaryCalculationResponseDto
    {
        public decimal GrossIncome { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalRatePercent { get; set; }
        public decimal NetIncome { get; set; }
    }
}
