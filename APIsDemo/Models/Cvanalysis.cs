using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

[Table("CVAnalysis")]
public partial class Cvanalysis
{
    [Key]
    public int Id { get; set; }

    [Column("CVId")]
    public int Cvid { get; set; }

    [StringLength(150)]
    public string? BestJob { get; set; }

    public string? SkillsImprovements { get; set; }

    public string? Summary { get; set; }

    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Analysis")]
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    [ForeignKey("Cvid")]
    [InverseProperty("Cvanalyses")]
    public virtual Cvdocument Cv { get; set; } = null!;
}
