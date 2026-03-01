using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace APIsDemo.Models
{
    public class CvBatchUploadRequest
    {
        public List<IFormFile> Files { get; set; } = new();
        public Guid UserId { get; set; }
        public string JobDescription { get; set; } = string.Empty;
    }
}