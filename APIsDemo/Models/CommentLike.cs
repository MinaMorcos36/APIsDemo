using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

[Index("CommentId", "UserId", Name = "UQ__CommentL__12CC530FA4C0C8DF", IsUnique = true)]
public partial class CommentLike
{
    [Key]
    public int Id { get; set; }

    public int CommentId { get; set; }
    public int? AuthorId { get; set; }
    public string AuthorType { get; set; } // 'JobSeeker' | 'Recruiter'

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("CommentId")]
    [InverseProperty("CommentLikes")]
    public virtual Comment Comment { get; set; } = null!;
}
