using APIsDemo.Models;
using APIsDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APIsDemo.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly CareerChatService _chatService;
    private readonly AppDbContext _context;

    public ChatController(CareerChatService chatService, AppDbContext context)
    {
        _chatService = chatService;
        _context = context;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // ============================
    // 0️⃣ Start Conversation (Manual)
    // ============================
    [HttpPost("start")]
    public async Task<IActionResult> StartConversation()
    {
        var userId = GetUserId();

        var conversation = new ConversationModel
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Title = "New Chat"
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            conversation.Id,
            conversation.Title
        });
    }

    // ============================
    // 1️⃣ Send Message (Smart)
    // ============================
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] CareerChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required");

        var userId = GetUserId();

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId);

        // 🧠 Auto-create conversation if not exists
        if (conversation == null)
        {
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

        // 🔒 Security check
        if (conversation.UserId != userId)
            return Forbid();

        // 🧠 Get CV text (if provided)
        string? cvText = null;

        if (request.CvId.HasValue)
        {
            var cv = await _context.Cvs
                .FirstOrDefaultAsync(c => c.Id == request.CvId.Value && c.UserId == userId);

            if (cv != null)
                cvText = cv.RawText;
        }

        var reply = await _chatService.AskAsync(
            conversation.Id, // مهم هنا نستخدم ID الحقيقي
            request.Message,
            cvText
        );

        return Ok(new
        {
            conversationId = conversation.Id,
            reply
        });
    }

    // ============================
    // 2️⃣ Get Conversation
    // ============================
    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetConversation(int conversationId)
    {
        var userId = GetUserId();

        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
            return NotFound("Conversation not found");

        if (conversation.UserId != userId)
            return Forbid();

        return Ok(conversation);
    }

    // ============================
    // 3️⃣ My Conversations
    // ============================
    [HttpGet("my-conversations")]
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