using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProGrow.API.Models;

[Table("JobLikes")]
public partial class JobLike
{
    [Key]
    public int Id { get; set; }

    public int JobId { get; set; }

    public int AuthorId { get; set; }

    [StringLength(20)]
    public string? AuthorType { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("JobId")]
    [InverseProperty("JobLikes")]
    public virtual Job Job { get; set; } = null!;
}
