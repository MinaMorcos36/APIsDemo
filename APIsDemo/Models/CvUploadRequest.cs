using Microsoft.AspNetCore.Http;

namespace APIsDemo.Models
{
    public class CvUploadRequest
    {
        public IFormFile File { get; set; }
        public Guid UserId { get; set; }
    }
}
