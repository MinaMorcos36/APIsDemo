using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProGrow.API.Models;

public partial class LocationMode
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [InverseProperty("LocationMode")]
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
