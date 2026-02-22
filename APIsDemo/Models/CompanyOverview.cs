using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

public partial class CompanyOverview
{
    [Key]
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public int IndustryId { get; set; }

    [StringLength(50)]
    public string? Name { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public string Overview { get; set; } = null!;

    [StringLength(255)]
    public string? WebsiteUrl { get; set; }

    public string? PictureUrl { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("CompanyOverviews")]
    public virtual Company Company { get; set; } = null!;

    [ForeignKey("IndustryId")]
    [InverseProperty("CompanyOverviews")]
    public virtual Industry Industry { get; set; } = null!;
}
