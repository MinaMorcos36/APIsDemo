using Microsoft.AspNetCore.Http;

namespace ProGrow.API.Models
{
    public class CvUploadRequest
    {
        public IFormFile File { get; set; }
        public int UserId { get; set; }
    }
}
