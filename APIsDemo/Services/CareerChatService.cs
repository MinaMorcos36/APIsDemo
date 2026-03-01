using APIsDemo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace APIsDemo.Services;

public class CareerChatService
{
    private readonly IChatCompletionService _chat;

    // Temporary in-memory storage
    //private static readonly List<ChatMessageModel> _messages = new();
    private readonly AppDbContext _context;

    public CareerChatService(Kernel kernel,AppDbContext context)
    {
        _chat = kernel.GetRequiredService<IChatCompletionService>();
        _context =context;
    }

    public async Task<string> AskAsync(
    Guid conversationId,
    string userMessage,
    string? cvText = null)
    {
        // Save user message
        _context.ChatMessages.Add(new ChatMessageModel
        {
            ConversationId = conversationId,
            Role = "user",
            Content = userMessage,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Get all messages for this conversation
        var history = await _context.ChatMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        var chatHistory = new ChatHistory();

        chatHistory.AddSystemMessage("""
You are a professional career advisor and HR expert.
Give practical, structured, intelligent advice.
""");

        if (!string.IsNullOrEmpty(cvText))
        {
            chatHistory.AddSystemMessage($"CV Context:\n{cvText}");
        }

        foreach (var msg in history)
        {
            if (msg.Role == "user")
                chatHistory.AddUserMessage(msg.Content);
            else
                chatHistory.AddAssistantMessage(msg.Content);
        }

        var result = await _chat.GetChatMessageContentAsync(chatHistory);

        var reply = result.Content ?? "No response";

        // Save assistant reply
        _context.ChatMessages.Add(new ChatMessageModel
        {
            ConversationId = conversationId,
            Role = "assistant",
            Content = reply,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return reply;
    }
}