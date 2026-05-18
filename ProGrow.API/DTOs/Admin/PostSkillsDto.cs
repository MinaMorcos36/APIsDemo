using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Admin
{
    public class PostSkillsDto
    {
        [Required]
        [MinLength(1)]
        public List<string> Names { get; set; } = new List<string>();
    }
}
