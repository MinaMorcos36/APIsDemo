using ProGrow.API.Models;
using ProGrow.API.Services.Implementations.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

    // =========================================
    // 1?? Send Message (Main Endpoint)
    // =========================================
    [HttpPost]
    public async Task<IActionResult> SendMessage(
        [FromBody] CareerChatRequest request)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required");

        ConversationModel? conversation = null;

        // ===============================
        // Existing Conversation
        // ===============================
        if (request.ConversationId.HasValue)
        {
            conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.Id == request.ConversationId.Value);

            if (conversation == null)
                return NotFound("Conversation not found");

            if (conversation.UserId != userId)
                return Forbid();
        }

        // ===============================
        // Create New Conversation
        // ===============================
        else
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

        // ===============================
        // Get CV Text
        // ===============================
        string? cvText = null;

        if (request.CvId.HasValue)
        {
            var cv = await _context.Cvs
                .FirstOrDefaultAsync(c =>
                    c.Id == request.CvId.Value &&
                    c.UserId == userId);

            if (cv != null)
                cvText = cv.RawText;
        }

        // ===============================
        // Ask AI
        // ===============================
        var reply = await _chatService.AskAsync(
            conversation.Id,
            request.Message,
            cvText);

        return Ok(new
        {
            conversationId = conversation.Id,
            reply
        });
    }

    // =========================================
    // 2?? Get Conversation Messages
    // =========================================
    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetConversation(
        int conversationId)
    {
        var userId = GetUserId();

        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
            return NotFound();

        if (conversation.UserId != userId)
            return Forbid();

        return Ok(conversation);
    }

    // =========================================
    // 3?? Get My Conversations
    // =========================================
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


    // =========================================
    // 4?? General AI Chat
    // =========================================
    [HttpPost("general")]
    public async Task<IActionResult> GeneralChat(
        [FromBody] CareerChatRequest request)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required");

        ConversationModel? conversation = null;

        // ===============================
        // Existing Conversation
        // ===============================
        if (request.ConversationId.HasValue)
        {
            conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.Id == request.ConversationId.Value);

            if (conversation == null)
                return NotFound("Conversation not found");

            if (conversation.UserId != userId)
                return Forbid();
        }

        // ===============================
        // Create New Conversation
        // ===============================
        else
        {
            conversation = new ConversationModel
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Title = request.Message.Length > 30
                    ? request.Message[..30]
                    : request.Message
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
        }

        // ===============================
        // Ask AI (???? CV)
        // ===============================
        var reply = await _chatService.AskAsync(
            conversation.Id,
            request.Message,
            null
        );

        return Ok(new
        {
            conversationId = conversation.Id,
            reply
        });
    }
}
