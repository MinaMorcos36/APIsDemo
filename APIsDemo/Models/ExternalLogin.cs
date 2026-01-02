using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

public partial class ExternalLogin
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(50)]
    public string Provider { get; set; } = null!;

    [StringLength(200)]
    public string ProviderKey { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("ExternalLogins")]
    public virtual User User { get; set; } = null!;
}
