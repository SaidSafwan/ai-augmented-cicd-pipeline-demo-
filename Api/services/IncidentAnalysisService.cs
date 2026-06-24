using Api.Models;
using Azure;
using Azure.AI.OpenAI;
using System.Text.Json;

namespace Api.services
{
    public class IncidentAnalysisService
    {
        private readonly IConfiguration _configuration;

        public IncidentAnalysisService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IncidentAnalysisResponse> Analyze(
        IncidentAnalysisRequest request)
        {
            var endpoint =
                _configuration["AZURE_OPENAI_ENDPOINT"];

            var key =
                _configuration["AZURE_OPENAI_API_KEY"];

            var deployment =
                _configuration["AZURE_OPENAI_DEPLOYMENT_NAME"];

            var client = new AzureOpenAIClient(
                new Uri(endpoint),
                new AzureKeyCredential(key));

            var chatClient =
                client.GetChatClient(deployment);

            var prompt = $@"
                Analyze this production incident.

                Error:
                {request.Error}

                Logs:
                {request.Logs}

                Return ONLY valid JSON.

                {{
                  ""severity"": ""High"",
                  ""rootCause"": ""..."",
                  ""recommendation"": ""...""
                }}

                Do not wrap the response in markdown.
                Do not use ```json.
                ";

            var response =
                await chatClient.CompleteChatAsync(prompt);

            string result =
                response.Value.Content[0].Text;

            var analysis =
                JsonSerializer.Deserialize<IncidentAnalysisResponse>(result);

            return analysis!;
        }
    }
}
