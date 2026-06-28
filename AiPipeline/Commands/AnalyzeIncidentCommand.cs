using System.Text;
using AiPipeline.Models;

namespace AiPipeline.Commands;

/// <summary>
/// `analyze-incident --log &lt;file&gt;` — runs only when the pipeline fails. Feeds the
/// captured logs to the model and produces a severity / root-cause / recommendation
/// summary. Advisory only (always exit 0) so the original failure stays the source of truth.
/// </summary>
public static class AnalyzeIncidentCommand
{
    public static async Task<int> RunAsync(IReadOnlyDictionary<string, string> args)
    {
        var logPath = args.GetValueOrDefault("log");
        var log = "";
        if (!string.IsNullOrWhiteSpace(logPath) && File.Exists(logPath))
        {
            log = (await File.ReadAllTextAsync(logPath)).Trim();
        }

        // Optional extra context passed directly (e.g. an error string).
        var error = args.GetValueOrDefault("error") ?? "";

        if (log.Length == 0 && error.Length == 0)
        {
            StepSummary.Write("## 🔍 AI Incident Analysis\n\n> No logs available to analyze.");
            return 0;
        }

        const int maxChars = 20000;
        if (log.Length > maxChars) log = log.Substring(log.Length - maxChars);

        var prompt = $@"
You are an experienced Site Reliability Engineer analyzing a failed CI/CD pipeline run.

Error (if any):
{error}

Logs:
{log}

Return ONLY valid JSON (no markdown, no prose) in EXACTLY this shape:
{{
  ""severity"": ""High"",
  ""rootCause"": ""..."",
  ""recommendation"": ""...""
}}
";

        IncidentResult? result;
        try
        {
            var client = AzureOpenAiClient.Create();
            var raw = await AiHelpers.CompleteAsync(client, prompt);
            result = AiHelpers.ParseJson<IncidentResult>(raw)
                     ?? new IncidentResult { Severity = "Unknown", RootCause = raw.Trim(), Recommendation = "See raw output above." };
        }
        catch (Exception ex)
        {
            StepSummary.Write(
                "## 🔍 AI Incident Analysis\n\n" +
                $"> ⚠️ Incident analysis could not run ({ex.Message}).");
            return 0;
        }

        var sb = new StringBuilder();
        sb.AppendLine("## 🔍 AI Incident Analysis");
        sb.AppendLine();
        sb.AppendLine($"**Severity:** {result.Severity}");
        sb.AppendLine();
        sb.AppendLine($"**Root Cause:** {result.RootCause}");
        sb.AppendLine();
        sb.AppendLine($"**Recommendation:** {result.Recommendation}");
        StepSummary.Write(sb.ToString());
        return 0;
    }
}
