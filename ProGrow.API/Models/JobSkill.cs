using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProGrow.API.Models;

[PrimaryKey("JobId", "SkillId")]
[Table("JobSkill")]
public partial class JobSkill
{
    [Key]
    public int JobId { get; set; }

    [Key]
    public int SkillId { get; set; }

    [ForeignKey("JobId")]
    [InverseProperty("JobSkills")]
    public virtual Job Job { get; set; } = null!;

    [ForeignKey("SkillId")]
    [InverseProperty("JobSkills")]
    public virtual Skill Skill { get; set; } = null!;
}