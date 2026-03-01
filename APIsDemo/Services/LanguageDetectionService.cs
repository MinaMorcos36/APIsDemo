namespace APIsDemo.Services;

public class LanguageDetectionService
{
    public string Detect(string text)
    {
        return text.Any(c => c >= 0x0600 && c <= 0x06FF)
            ? "AR"
            : "EN";
    }
}
