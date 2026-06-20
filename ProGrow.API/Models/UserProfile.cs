using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProGrow.API.Models;

[Index("UserId", IsUnique = true, Name = "UQ_UserProfiles_UserId")]
public partial class UserProfile
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(60)]
    public string? FirstName { get; set; }

    [StringLength(60)]
    public string? LastName { get; set; }

    public DateOnly? Birthdate { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }

    [StringLength(100)]
    public string? Headline { get; set; }

    [StringLength(100)]
    public string? Major { get; set; }

    [StringLength(100)]
    public string? University { get; set; }

    public string? PictureUrl { get; set; }

    [Column("CVScore")]
    public int? CvScore { get; set; }

    [Column("CVName")]
    public string? CvName { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserProfiles")]
    public virtual User User { get; set; } = null!;
}
