using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProGrow.API.Models;

public partial class JobApplication
{
    [Key]
    public int Id { get; set; }

    public int JobId { get; set; }

    public int ApplicantId { get; set; }

    public int StatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string CvFileName { get; set; } = null!;

    public string? CvFilePath { get; set; }

    public int? CvId { get; set; }

    public int? CvScore { get; set; }

    public string? CvScoreReason { get; set; }

    public string? CoverLetter { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string? PortfolioLink { get; set; }

    [ForeignKey("JobId")]
    [InverseProperty("JobApplications")]
    public virtual Job Job { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("JobApplications")]
    public virtual JobApplicationStatus Status { get; set; } = null!;

    [ForeignKey("CvId")]
    public virtual CvModel? Cv { get; set; }
}
