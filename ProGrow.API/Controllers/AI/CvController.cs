using ProGrow.API.Models;
using ProGrow.API.Services.Implementations.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ProGrow.API.Controllers.AI;

[Authorize]
[ApiController]
[Route("api/cv")]
public class CvController : ControllerBase
{
    private readonly CvProcessingService _cvService;
    private readonly FileParsingService _fileParsingService;
    private readonly GeminiCvEvaluationService _geminiService;
    private readonly CareerChatService _careerChatService;
    private readonly AppDbContext _context;

    public CvController(
        CvProcessingService cvService,
        FileParsingService fileParsingService,
        GeminiCvEvaluationService geminiService,
        CareerChatService careerChatService,
        AppDbContext context)
    {
        _cvService = cvService;
        _fileParsingService = fileParsingService;
        _geminiService = geminiService;
        _careerChatService = careerChatService;
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated");

        return int.Parse(userIdClaim);
    }

    // ===============================
    // 1?? Upload CV
    // ===============================
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] CvUploadRequest dto)
    {
        var userId = GetUserId();

        if (dto.File == null || dto.File.Length == 0)
            return BadRequest("File is required");

        string text;
        try
        {
            text = _fileParsingService.Parse(dto.File);
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(ex.Message);
        }

        var cv = await _cvService.Save(userId, dto.File.FileName, text);

        return Ok(new
        {
            cv.Id,
            cv.FileName,
            cv.Language,
            cv.CreatedAt
        });
    }

    // ===============================
    // 2?? Upload + Evaluate (SAFE)
    // ===============================
    [HttpPost("upload-and-evaluate")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAndEvaluate(
        [FromForm] CvUploadRequest dto,
        [FromForm] string jobDescription)
    {
        var userId = GetUserId();

        if (dto.File == null || dto.File.Length == 0)
            return BadRequest("File is required");

        if (string.IsNullOrWhiteSpace(jobDescription))
            return BadRequest("Job description is required");

        string text;
        try
        {
            text = _fileParsingService.Parse(dto.File);
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(ex.Message);
        }

        var cv = await _cvService.Save(userId, dto.File.FileName, text);

        object? evaluation = null;
        string? aiError = null;

        try
        {
            evaluation = await _geminiService
                .EvaluateAsync(cv.RawText, jobDescription);
        }
        catch (Exception)
        {
            aiError = "AI service is currently unavailable";
        }

        return Ok(new
        {
            cv.Id,
            cv.FileName,
            cv.Language,
            cv.CreatedAt,

            Score = evaluation?.GetType().GetProperty("Score")?.GetValue(evaluation),
            Reason = evaluation?.GetType().GetProperty("Reason")?.GetValue(evaluation),
            Shortlisted = evaluation?.GetType().GetProperty("Shortlisted")?.GetValue(evaluation),

            AiError = aiError
        });
    }

    // ===============================
    // Score a CV using user's profile headline and save score to profile
    // ===============================
    [Authorize(Policy = "JobSeekerOnly")]
    [HttpPost("score/{cvId}")]
    public async Task<IActionResult> ScoreCv(int cvId)
    {
        var userId = GetUserId();

        var cv = await _cvService.GetById(cvId, userId);
        if (cv == null)
            return NotFound("CV not found.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            profile = new UserProfile { UserId = userId };
            _context.UserProfiles.Add(profile);
        }

        var jobDescription = profile.Headline;
        if (string.IsNullOrWhiteSpace(jobDescription))
            return BadRequest("Profile headline is required for scoring.");

        CvEvaluationResult evaluation;
        try
        {
            evaluation = await _geminiService.EvaluateAsync(cv.RawText, jobDescription);
        }
        catch (Exception)
        {
            return StatusCode(503, "AI service is currently unavailable");
        }

        profile.CvScore = evaluation.Score;
        profile.CvName = cv.FileName;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            FileName = cv.FileName,
            Score = evaluation.Score,
            Reason = evaluation.Reason,
            Shortlisted = evaluation.Shortlisted
        });
    }

    // ===============================
    // 3?? Get My CVs
    // ===============================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var cvs = await _cvService.GetAllByUser(userId);

        return Ok(cvs.Select(c => new
        {
            c.Id,
            c.FileName,
            c.Language,
            c.CreatedAt
        }));
    }

    // ===============================
    // 3.1?? Get My CVs for Dropdown
    // ===============================
    [Authorize(Policy = "JobSeekerOnly")]
    [HttpGet("my-cvs")]
    public async Task<IActionResult> GetMyCvsForDropdown()
    {
        var userId = GetUserId();

        var cvs = await _context.Cvs
            .Where(cv => cv.UserId == userId)
            .OrderByDescending(cv => cv.CreatedAt)
            .Select(cv => new
            {
                cv.Id,
                cv.FileName
            })
            .ToListAsync();

        return Ok(cvs);
    }

    // ===============================
    // 4?? Batch Upload (SAFE)
    // ===============================
    [HttpPost("upload-and-evaluate-batch")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAndEvaluateBatch([FromForm] CvBatchUploadRequest dto)
    {
        var userId = GetUserId();

        if (dto.Files == null || !dto.Files.Any())
            return BadRequest("At least one file is required");

        if (string.IsNullOrWhiteSpace(dto.JobDescription))
            return BadRequest("Job description is required");

        var results = new List<object>();

        foreach (var file in dto.Files)
        {
            try
            {
                var text = _fileParsingService.Parse(file);
                var cv = await _cvService.Save(userId, file.FileName, text);

                object? evaluation = null;
                string? aiError = null;

                try
                {
                    evaluation = await _geminiService
                        .EvaluateAsync(cv.RawText, dto.JobDescription);
                }
                catch (Exception)
                {
                    aiError = "AI service is currently unavailable";
                }

                results.Add(new
                {
                    cv.Id,
                    cv.FileName,
                    cv.Language,
                    cv.CreatedAt,

                    Score = evaluation?.GetType().GetProperty("Score")?.GetValue(evaluation),
                    Reason = evaluation?.GetType().GetProperty("Reason")?.GetValue(evaluation),
                    Shortlisted = evaluation?.GetType().GetProperty("Shortlisted")?.GetValue(evaluation),

                    AiError = aiError
                });
            }
            catch (Exception ex)
            {
                results.Add(new
                {
                    FileName = file.FileName,
                    Error = ex.Message
                });
            }
        }

        return Ok(results);
    }
   
}
