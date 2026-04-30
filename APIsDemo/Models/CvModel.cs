using APIsDemo.Models;

public class CvModel
{
    public int Id { get; set; }

    public int UserId { get; set; }   // FK only

    public User User { get; set; }    // navigation

    public string FileName { get; set; }
    public string RawText { get; set; }
    public string Language { get; set; }

    public DateTime CreatedAt { get; set; }
}