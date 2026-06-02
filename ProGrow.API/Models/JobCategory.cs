using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProGrow.API.Models
{
    public partial class JobCategory
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; } = null!;

        [InverseProperty(nameof(Job.JobCategory))]
        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
