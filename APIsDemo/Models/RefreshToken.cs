using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

[Index("Token", Name = "UQ__RefreshT__1EB4F8170BEA11F2", IsUnique = true)]
public partial class RefreshToken
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(256)]
    public string Token { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? ExpiresAt { get; set; }

    public bool? IsRevoked { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("RefreshTokens")]
    public virtual User User { get; set; } = null!;
}
