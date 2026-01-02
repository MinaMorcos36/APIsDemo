using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

public partial class Conversation
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? AnalysisId { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("AnalysisId")]
    [InverseProperty("Conversations")]
    public virtual Cvanalysis? Analysis { get; set; }

    [InverseProperty("Convo")]
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
