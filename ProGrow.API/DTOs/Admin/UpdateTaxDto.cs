using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Admin
{
    public class UpdateTaxDto
    {
        [Range(0, 100)]
        public decimal Percentage { get; set; }
    }
}
