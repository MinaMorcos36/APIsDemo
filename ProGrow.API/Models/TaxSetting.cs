using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.Models
{
    public class TaxSetting
    {
        [Key]
        public int Id { get; set; }
        public decimal Percentage { get; set; }
    }
}
