namespace APIsDemo.Models
{
    public class CvModel
    {

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FileName { get; set; }
        public string RawText { get; set; }
        public string Language { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
