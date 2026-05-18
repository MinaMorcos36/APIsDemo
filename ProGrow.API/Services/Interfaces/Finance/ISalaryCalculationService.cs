using ProGrow.API.DTOs.Finance;

namespace ProGrow.API.Services.Interfaces.Finance
{
    public interface ISalaryCalculationService
    {
        SalaryCalculationResponseDto Calculate(SalaryCalculationRequestDto dto);
    }
}
