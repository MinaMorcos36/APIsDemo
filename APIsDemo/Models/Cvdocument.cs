using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

[Table("CVDocuments")]
public partial class Cvdocument
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(500)]
    public string BlobUrl { get; set; } = null!;

    [StringLength(255)]
    public string? OriginalFileName { get; set; }

    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Cv")]
    public virtual ICollection<Cvanalysis> Cvanalyses { get; set; } = new List<Cvanalysis>();
}
