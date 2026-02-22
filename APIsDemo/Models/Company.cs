using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

[Index("Email", Name = "UQ__Company__A9D1053468DF0116", IsUnique = true)]
public partial class Company
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool IsVerified { get; set; }

    [Column("OTP")]
    [StringLength(6)]
    public string? Otp { get; set; }

    [Column("OTPExpiry", TypeName = "datetime")]
    public DateTime? Otpexpiry { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("Company")]
    public virtual ICollection<CompanyOverview> CompanyOverviews { get; set; } = new List<CompanyOverview>();

    [InverseProperty("Company")]
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
