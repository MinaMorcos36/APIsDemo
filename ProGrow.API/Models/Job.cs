using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProGrow.API.Models;

public partial class Job
{
    [Key]
    public int Id { get; set; }

    public int CompanyId { get; set; }

    [StringLength(150)]
    public string Title { get; set; } = null!;

    [StringLength(100)]
    public string ShortDescription { get; set; } = null!;

    [StringLength(20)]
    public string LocationMode { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }

    [StringLength(20)]
    public string JobType { get; set; } = null!;

    [StringLength(150)]
    public string CityOffice { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SalaryFrom { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SalaryTo { get; set; }

    public bool SalaryInInterview { get; set; }

    public string BannerImageUrl { get; set; } = null!;

    public string AboutRole { get; set; } = null!;

    public string Responsibilities { get; set; } = null!;

    public string Requirements { get; set; } = null!;

    [ForeignKey("CompanyId")]
    [InverseProperty("Jobs")]
    public virtual Company Company { get; set; } = null!;

    [InverseProperty("Job")]
    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();

    [InverseProperty("Job")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("Job")]
    public virtual ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();

    [InverseProperty("Job")]
    public virtual ICollection<JobLike> JobLikes { get; set; } = new List<JobLike>();

    [InverseProperty("Job")]
    public virtual ICollection<JobSave> JobSaves { get; set; } = new List<JobSave>();
}
