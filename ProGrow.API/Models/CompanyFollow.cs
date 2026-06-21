using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProGrow.API.Models;

public partial class CompanyFollow
{
    [Key]
    public int Id { get; set; }

    public int CompanyId { get; set; } // follower (source)

    public int? FollowedUserId { get; set; }

    public int? FollowedCompanyId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; } = null!;

    [ForeignKey("FollowedUserId")]
    public virtual User? FollowedUser { get; set; }

    [ForeignKey("FollowedCompanyId")]
    public virtual Company? FollowedCompany { get; set; }
}
