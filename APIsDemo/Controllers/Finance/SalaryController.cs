using APIsDemo.DTOs.Finance;
using APIsDemo.Services.Interfaces.Finance;
using Microsoft.AspNetCore.Mvc;

namespace APIsDemo.Controllers.Finance
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
