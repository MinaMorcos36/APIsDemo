using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

public partial class Comment
{
    [Key]
    public int Id { get; set; }

    public int PostId { get; set; }

    public int AuthorId { get; set; }

    [StringLength(20)]
    public string? AuthorType { get; set; }

    public int? ParentCommentId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Comment")]
    public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();

    [InverseProperty("ParentComment")]
    public virtual ICollection<Comment> InverseParentComment { get; set; } = new List<Comment>();

    [ForeignKey("ParentCommentId")]
    [InverseProperty("InverseParentComment")]
    public virtual Comment? ParentComment { get; set; }

    [ForeignKey("PostId")]
    [InverseProperty("Comments")]
    public virtual Post Post { get; set; } = null!;
}
