using APIsDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Services;

public class CvProcessingService
{
    private readonly LanguageDetectionService _langService;
    private readonly AppDbContext _context;

    public CvProcessingService(
        LanguageDetectionService langService,
        AppDbContext context)
    {
        _langService = langService;
        _context = context;
    }

    // ============================
    // Save CV in Database
    // ============================
    public async Task<CvModel> Save(int userId, string fileName, string text)
    {
        var cv = new CvModel
        {
            UserId = userId,
            FileName = fileName,
            RawText = text,
            Language = _langService.Detect(text),
            CreatedAt = DateTime.UtcNow
        };

        _context.Cvs.Add(cv);
        await _context.SaveChangesAsync();

        return cv;
    }

    // ============================
    // Get all CVs for a user
    // ============================
    public async Task<List<CvModel>> GetAllByUser(int userId)
    {
        return await _context.Cvs
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    // ============================
    // Get CV by Id (with security)
    // ============================
    public async Task<CvModel?> GetById(int cvId, int userId)
    {
        return await _context.Cvs
            .FirstOrDefaultAsync(c => c.Id == cvId && c.UserId == userId);
    }
}