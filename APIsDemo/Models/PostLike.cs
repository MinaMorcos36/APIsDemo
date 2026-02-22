using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

public partial class PostLike
{
    [Key]
    public int Id { get; set; }

    public int PostId { get; set; }

    public int AuthorId { get; set; }

    [StringLength(20)]
    public string? AuthorType { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("PostId")]
    [InverseProperty("PostLikes")]
    public virtual Post Post { get; set; } = null!;
}
