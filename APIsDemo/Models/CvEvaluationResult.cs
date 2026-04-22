namespace APIsDemo.Models;

public class CvEvaluationResult
{
    public int Score { get; set; }
    public string Reason { get; set; } = string.Empty;

    public bool Shortlisted => Score >= 70;
}
