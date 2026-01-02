using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

public partial class Message
{
    [Key]
    public int Id { get; set; }

    public int ConvoId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("ConvoId")]
    [InverseProperty("Messages")]
    public virtual Conversation Convo { get; set; } = null!;
}
