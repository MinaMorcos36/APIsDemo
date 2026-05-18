using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.Models
{
    public class CvBatchUploadRequest
    {
        [Required]
        [MinLength(1)]
        public List<IFormFile> Files { get; set; } = new();
        public int UserId { get; set; }
        [Required]
        [StringLength(4000, MinimumLength = 1)]
        [RegularExpression(@".*\S.*")]
        public string JobDescription { get; set; } = string.Empty;
    }
}
