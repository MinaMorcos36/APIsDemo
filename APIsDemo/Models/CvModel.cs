namespace APIsDemo.Models
{
    public class CvModel
    {

        public int Id { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; }
        public string RawText { get; set; }
        public string Language { get; set; }
        public DateTime CreatedAt { get; set; }
        
        

      
    }
}
