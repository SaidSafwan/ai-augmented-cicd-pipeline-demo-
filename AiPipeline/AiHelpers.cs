using System.Text.Json;
using OpenAI.Chat;

namespace AiPipeline;

/// <summary>Shared helpers for talking to the model and parsing its JSON replies.</summary>
public static class AiHelpers
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    { 
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Single-shot completion returning the raw text.</summary>
    public static async Task<string> CompleteAsync(ChatClient client, string prompt)
    {
        var response = await client.CompleteChatAsync(prompt);
        return response.Value.Content[0].Text;
    }

    /// <summary>
    /// Strips Markdown code fences the model sometimes wraps JSON in, then deserializes.
    /// (Same cleanup the old AiEngineeringAssistantService used.) Returns null on failure
    /// so callers can fall back gracefully.
    /// </summary>
    public static T? ParseJson<T>(string raw) where T : class
    {
        var cleaned = raw
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        try
        {
            return JsonSerializer.Deserialize<T>(cleaned, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
