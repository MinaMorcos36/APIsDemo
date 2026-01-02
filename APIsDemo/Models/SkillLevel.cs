using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

[Table("SkillLevel")]
[Index("Name", Name = "UQ__SkillLev__737584F64F9359E8", IsUnique = true)]
public partial class SkillLevel
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [InverseProperty("Level")]
    public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
}
