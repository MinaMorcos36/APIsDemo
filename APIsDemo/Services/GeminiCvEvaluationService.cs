using APIsDemo.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace APIsDemo.Services;

public class GeminiCvEvaluationService
{
    private readonly IChatCompletionService _chat;

    public GeminiCvEvaluationService(Kernel kernel)
    {
        _chat = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<CvEvaluationResult> EvaluateAsync(
        string cvText,
        string jobDescription)
    {
        var prompt = $$"""
You are a professional HR recruiter.

Evaluate the following CV against the Job Description.

IMPORTANT RULES:
- Return ONLY valid JSON
- Do NOT add any text before or after JSON
- Do NOT use markdown
- Do NOT explain anything

Return JSON exactly like this example:
{
  "score": 85,
  "reason": "Missing required backend experience"
}

Job Description:
{{jobDescription}}

CV:
{{cvText}}
""";


        var result = await _chat.GetChatMessageContentAsync(prompt);

        try
        {
            return JsonSerializer.Deserialize<CvEvaluationResult>(
                result.Content!,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            )!;
        }
        catch
        {
            // Fallback آمن في حالة Gemini رجّع رد مش قابل للـ parsing
            return new CvEvaluationResult
            {
                Score = 0,
                Reason = "AI response could not be parsed"
            };
        }
    }
}
