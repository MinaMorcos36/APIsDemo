using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

public partial class JobApplication
{
    [Key]
    public int Id { get; set; }

    public int JobId { get; set; }

    public int ApplicantId { get; set; }

    public int StatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("JobId")]
    [InverseProperty("JobApplications")]
    public virtual Job Job { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("JobApplications")]
    public virtual JobApplicationStatus Status { get; set; } = null!;
}
