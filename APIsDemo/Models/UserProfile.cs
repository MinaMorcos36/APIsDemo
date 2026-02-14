using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

[Table("UserProfile")]
public partial class UserProfile
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }

    [StringLength(100)]
    public string? Headline { get; set; }

    [StringLength(100)]
    public string? Major { get; set; }

    [StringLength(100)]
    public string? University { get; set; }

    public string? PictureUrl { get; set; }

    [Column("CVUrl")]
    public string? Cvurl { get; set; }

    [StringLength(60)]
    public string? FirstName { get; set; }

    [StringLength(60)]
    public string? LastName { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    public DateOnly? Birthdate { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }
}
