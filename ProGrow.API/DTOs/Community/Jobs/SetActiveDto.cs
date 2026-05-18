using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.DTOs.Community.Jobs
{
    public class SetActiveDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
