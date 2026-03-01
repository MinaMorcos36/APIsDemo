using APIsDemo.Models;

namespace APIsDemo.Services;

public class CvProcessingService
{
    private static readonly List<CvModel> _cvs = new();

    private readonly LanguageDetectionService _langService;

    public CvProcessingService(LanguageDetectionService langService)
    {
        _langService = langService;
    }

    public CvModel Save(Guid userId, string fileName, string text)
    {
        var cv = new CvModel
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FileName = fileName,
            RawText = text,
            Language = _langService.Detect(text),
            CreatedAt = DateTime.UtcNow
        };

        _cvs.Add(cv);
        return cv;
    }

    public List<CvModel> GetAll() => _cvs;
}
