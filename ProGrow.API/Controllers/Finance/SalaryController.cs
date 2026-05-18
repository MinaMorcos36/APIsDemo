using ProGrow.API.DTOs.Finance;
using ProGrow.API.Services.Interfaces.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ProGrow.API.Controllers.Finance
{
    [ApiController]
    [Route("api/finance")]
    public class SalaryController : ControllerBase
    {
        private readonly ISalaryCalculationService _salaryCalculationService;

        public SalaryController(ISalaryCalculationService salaryCalculationService)
        {
            _salaryCalculationService = salaryCalculationService;
        }

        [HttpPost("salary/calculate")]
        public IActionResult CalculateSalary([FromBody] SalaryCalculationRequestDto dto)
        {
            try
            {
                var result = _salaryCalculationService.Calculate(dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
