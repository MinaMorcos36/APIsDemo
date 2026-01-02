using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

[Table("ApplicationStatus")]
[Index("Name", Name = "UQ__Applicat__737584F615BEDFCC", IsUnique = true)]
public partial class ApplicationStatus
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string? Name { get; set; }

    [InverseProperty("Status")]
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
}
