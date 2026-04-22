using APIsDemo.Models;
using APIsDemo.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIsDemo.Controllers;

[ApiController]
[Route("api/cv")]
public class CvController : ControllerBase
{
    private readonly CvProcessingService _cvService;
    private readonly FileParsingService _fileParsingService;
    private readonly GeminiCvEvaluationService _geminiService;
    private readonly CareerChatService _careerChatService;

    public CvController(
        CvProcessingService cvService,
        FileParsingService fileParsingService,
        GeminiCvEvaluationService geminiService,
        CareerChatService careerChatService)
    {
        _cvService = cvService;
        _fileParsingService = fileParsingService;
        _geminiService = geminiService;
        _careerChatService = careerChatService;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // ===============================
    // 1️⃣ Upload CV
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
    // 2️⃣ Upload + Evaluate
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

        var evaluation = await _geminiService
            .EvaluateAsync(cv.RawText, jobDescription);

        return Ok(new
        {
            cv.Id,
            cv.FileName,
            cv.Language,
            cv.CreatedAt,
            Score = evaluation.Score,
            evaluation.Reason,
            evaluation.Shortlisted
        });
    }

    // ===============================
    // 3️⃣ Get My CVs
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
    // 4️⃣ Batch Upload
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
            string text;
            try
            {
                text = _fileParsingService.Parse(file);
            }
            catch (NotSupportedException ex)
            {
                results.Add(new
                {
                    FileName = file.FileName,
                    Error = ex.Message
                });
                continue;
            }

            var cv = await _cvService.Save(userId, file.FileName, text);

            var evaluation = await _geminiService
                .EvaluateAsync(cv.RawText, dto.JobDescription);

            results.Add(new
            {
                cv.Id,
                cv.FileName,
                cv.Language,
                cv.CreatedAt,
                Score = evaluation.Score,
                evaluation.Reason,
                evaluation.Shortlisted
            });
        }

        return Ok(results);
    }

    // ===============================
    // 5️⃣ Career Chat
    // ===============================
    [HttpPost("career-chat")]
    public async Task<IActionResult> CareerChat([FromBody] CareerChatRequest request)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required");

        string? cvText = null;

        if (request.CvId.HasValue)
        {
            var cv = await _cvService.GetById(request.CvId.Value, userId);

            if (cv != null)
                cvText = cv.RawText;
        }

        var response = await _careerChatService.AskAsync(
            request.ConversationId,
            request.Message,
            cvText);

        return Ok(new
        {
            Reply = response
        });
    }
}