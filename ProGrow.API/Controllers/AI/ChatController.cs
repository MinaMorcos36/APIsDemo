using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProGrow.API.Models;
using ProGrow.API.Services.Implementations.AI;
using System.Security.Claims;
using UglyToad.PdfPig;
using System.Linq;

namespace ProGrow.API.Controllers.AI;

[Authorize]
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly CareerChatService _chatService;
    private readonly AppDbContext _context;

    public ChatController(
        CareerChatService chatService,
        AppDbContext context)
    {
        _chatService = chatService;
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated");

        return int.Parse(userIdClaim);
    }

    // =========================
    // Extract PDF Text
    // =========================
    private string ExtractPdfText(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var document = PdfDocument.Open(stream);

        return string.Join("\n",
            document.GetPages().Select(p => p.Text));
    }

    // =========================
    // MAIN CHAT ENDPOINT
    // =========================
    [HttpPost]
    public async Task<IActionResult> SendMessage(
        [FromForm] ChatRequest request)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required");

        ConversationModel? conversation = null;

        // =========================
        // Existing Conversation
        // =========================
        if (request.ConversationId.HasValue)
        {
            conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.Id == request.ConversationId.Value &&
                    c.UserId == userId);

            if (conversation == null)
                return NotFound("Conversation not found");
        }
        else
        {
            // =========================
            // New Conversation
            // =========================
            conversation = new ConversationModel
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Title = request.Message.Length > 30
                    ? request.Message.Substring(0, 30)
                    : request.Message
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
        }

        string? cvText = null;

        // =========================
        // Upload CV (PDF -> TEXT)
        // =========================
        if (request.File != null && request.File.Length > 0)
        {
            cvText = ExtractPdfText(request.File);

            var cv = new CvModel
            {
                UserId = userId,
                FileName = request.File.FileName,
                RawText = cvText,
                CreatedAt = DateTime.UtcNow,
                Language = null // مهم عشان error اللي كان عندك
            };

            _context.Cvs.Add(cv);
            await _context.SaveChangesAsync();

            conversation.CvId = cv.Id;
            await _context.SaveChangesAsync();
        }
        else if (conversation.CvId.HasValue)
        {
            var cv = await _context.Cvs
                .FirstOrDefaultAsync(c =>
                    c.Id == conversation.CvId.Value &&
                    c.UserId == userId);

            if (cv != null)
                cvText = cv.RawText;
        }

        // =========================
        // AI CALL
        // =========================
        var reply = await _chatService.AskAsync(
            conversation.Id,
            request.Message,
            cvText
        );

        return Ok(new
        {
            conversationId = conversation.Id,
            reply
        });
    }

    // =========================
    // GET CONVERSATION
    // =========================
    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetConversation(int conversationId)
    {
        var userId = GetUserId();

        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c =>
                c.Id == conversationId &&
                c.UserId == userId);

        if (conversation == null)
            return NotFound();

        return Ok(conversation);
    }

    // =========================
    // GET MY CONVERSATIONS
    // =========================
    [HttpGet]
    public async Task<IActionResult> GetMyConversations()
    {
        var userId = GetUserId();

        var conversations = await _context.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.CreatedAt
            })
            .ToListAsync();

        return Ok(conversations);
    }
}