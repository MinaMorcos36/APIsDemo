using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

[Table("Company")]
[Index("Email", Name = "UQ__Company__A9D1053468DF0116", IsUnique = true)]
public partial class Company
{
    [Key]
    public int Id { get; set; }

    public int IndustryId { get; set; }

    [StringLength(150)]
    public string Name { get; set; } = null!;

    [StringLength(150)]
    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool IsVerified { get; set; }

    [Column("OTP")]
    [StringLength(6)]
    public string? Otp { get; set; }

    [Column("OTPExpiry", TypeName = "datetime")]
    public DateTime? Otpexpiry { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(255)]
    public string? WebsiteUrl { get; set; }

    public bool IsActive { get; set; }

    public string Overview { get; set; } = null!;

    public string? PictureUrl { get; set; }

    [ForeignKey("IndustryId")]
    [InverseProperty("Companies")]
    public virtual Industry Industry { get; set; } = null!;

    [InverseProperty("Company")]
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
