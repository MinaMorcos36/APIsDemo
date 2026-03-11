using System.ComponentModel.DataAnnotations;

namespace APIsDemo.Models
{
    public class TaxSetting
    {
        [Key]
        public int Id { get; set; }
        public decimal Percentage { get; set; }
    }
}
