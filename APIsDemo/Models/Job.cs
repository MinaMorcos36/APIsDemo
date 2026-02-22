using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

public partial class Job
{
    [Key]
    public int Id { get; set; }

    public int CompanyId { get; set; }

    [StringLength(150)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [StringLength(150)]
    public string? Location { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("Jobs")]
    public virtual Company Company { get; set; } = null!;

    [InverseProperty("Job")]
    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}
