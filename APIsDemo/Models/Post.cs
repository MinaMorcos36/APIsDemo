using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

[Table("Post")]
public partial class Post
{
    [Key]
    public int Id { get; set; }

    public int AuthorId { get; set; }

    [StringLength(20)]
    public string? AuthorType { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Post")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("Post")]
    public virtual ICollection<LikedPost> LikedPosts { get; set; } = new List<LikedPost>();

    [InverseProperty("Post")]
    public virtual ICollection<SavedPost> SavedPosts { get; set; } = new List<SavedPost>();
}
