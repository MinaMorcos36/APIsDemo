using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ProGrow.API.Models
{
    public class CvUploadRequest
    {
        [Required]
        public IFormFile File { get; set; }
        public int UserId { get; set; }
    }
}
