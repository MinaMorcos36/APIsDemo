using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

[PrimaryKey("UserId", "SkillId")]
public partial class UserSkill
{
    [Key]
    public int UserId { get; set; }

    [Key]
    public int SkillId { get; set; }

    public int LevelId { get; set; }

    [ForeignKey("LevelId")]
    [InverseProperty("UserSkills")]
    public virtual SkillLevel Level { get; set; } = null!;

    [ForeignKey("SkillId")]
    [InverseProperty("UserSkills")]
    public virtual Skill Skill { get; set; } = null!;
}
