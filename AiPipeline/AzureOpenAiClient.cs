using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace AiPipeline;

/// <summary>
/// Builds a <see cref="ChatClient"/> from environment variables so GitHub Actions
/// secrets flow straight in. Fails fast with a clear message if anything is missing.
/// (Lifted from the old AiEngineeringAssistantService constructor.)
/// </summary>
public static class AzureOpenAiClient
{
    public static ChatClient Create()
    {
        var endpoint = RequireEnv("AZURE_OPENAI_ENDPOINT");
        var key = RequireEnv("AZURE_OPENAI_API_KEY");
        var deployment = RequireEnv("AZURE_OPENAI_DEPLOYMENT_NAME");

        var client = new AzureOpenAIClient(
            new Uri(endpoint),
            new AzureKeyCredential(key));

        return client.GetChatClient(deployment);
    }

    private static string RequireEnv(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing required environment variable '{name}'. " +
                "Set it as a GitHub Actions secret (and locally for testing).");
        }
        return value;
    }
}
