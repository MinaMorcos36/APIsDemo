using APIsDemo.DTOs.Finance;

namespace APIsDemo.Services.Interfaces.Finance
{
    public interface ISalaryCalculationService
    {
        SalaryCalculationResponseDto Calculate(SalaryCalculationRequestDto dto);
    }
}
