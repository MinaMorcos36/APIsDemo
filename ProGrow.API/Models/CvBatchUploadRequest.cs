using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ProGrow.API.Models
{
    public class CvBatchUploadRequest
    {
        public List<IFormFile> Files { get; set; } = new();
        public int UserId { get; set; }
        public string JobDescription { get; set; } = string.Empty;
    }
}
