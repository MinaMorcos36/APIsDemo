using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

[Table("Industry")]
[Index("Name", Name = "UQ__Industry__737584F66F3BB049", IsUnique = true)]
public partial class Industry
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [InverseProperty("Industry")]
    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
}
