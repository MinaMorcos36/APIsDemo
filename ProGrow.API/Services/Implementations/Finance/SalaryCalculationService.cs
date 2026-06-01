using ProGrow.API.DTOs.Finance;
using ProGrow.API.Services.Interfaces.Finance;

namespace ProGrow.API.Services.Implementations.Finance
{
    public class SalaryCalculationService : ISalaryCalculationService
    {
        private const decimal ExemptionAmount = 20000m;

        private static readonly (decimal Limit, decimal Rate)[] Brackets =
        [
            (40000m, 0m),
            (15000m, 0.10m),
            (15000m, 0.15m),
            (130000m, 0.20m),
            (200000m, 0.225m),
            (800000m, 0.25m),
            (decimal.MaxValue, 0.275m)
        ];

        public SalaryCalculationResponseDto Calculate(SalaryCalculationRequestDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (dto.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero.");
            }

            var isMonthly = string.Equals(dto.SalaryType, "monthly", StringComparison.OrdinalIgnoreCase);
            var isAnnual = string.Equals(dto.SalaryType, "annual", StringComparison.OrdinalIgnoreCase);

            if (!isMonthly && !isAnnual)
            {
                throw new ArgumentException("Salary type must be 'monthly' or 'annual'.");
            }

            var annualGross = isMonthly ? dto.Amount * 12m : dto.Amount;
            var taxableIncome = Math.Max(0m, annualGross - ExemptionAmount);
            var annualTax = CalculateTax(taxableIncome);
            var rate = annualGross == 0m ? 0m : Math.Round((annualTax / annualGross) * 100m, 2, MidpointRounding.AwayFromZero);
            var grossIncome = Math.Round(dto.Amount, 2, MidpointRounding.AwayFromZero);
            var totalTax = Math.Round(isMonthly ? annualTax / 12m : annualTax, 2, MidpointRounding.AwayFromZero);
            var netIncome = Math.Round(grossIncome - totalTax, 2, MidpointRounding.AwayFromZero);

            return new SalaryCalculationResponseDto
            {
                GrossIncome = grossIncome,
                TotalTax = totalTax,
                TotalRatePercent = rate,
                NetIncome = netIncome
            };
        }

        private static decimal CalculateTax(decimal taxableIncome)
        {
            var remaining = taxableIncome;
            var totalTax = 0m;

            foreach (var (limit, rate) in Brackets)
            {
                if (remaining <= 0m)
                {
                    break;
                }

                var taxableAtRate = Math.Min(remaining, limit);
                totalTax += taxableAtRate * rate;
                remaining -= taxableAtRate;
            }

            return totalTax;
        }
    }
}
