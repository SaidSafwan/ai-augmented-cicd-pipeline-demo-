using System.Text;
using AiPipeline.Models;

namespace AiPipeline.Commands;

/// <summary>
/// `validate-deploy --log &lt;file&gt;` — AI analysis of the captured build/publish/deploy log.
/// GATES the pipeline: exit 1 when the deployment is judged failed or carries a Critical
/// risk, else exit 0. Fails open (exit 0) on AI error.
/// </summary>
public static class ValidateDeployCommand
{
    public static async Task<int> RunAsync(IReadOnlyDictionary<string, string> args)
    {
        var logPath = args.GetValueOrDefault("log");
        if (string.IsNullOrWhiteSpace(logPath) || !File.Exists(logPath))
        {
            StepSummary.Write("## 🚀 AI Deployment Validation\n\n> No deployment log provided — skipped.");
            return 0;
        }

        var log = (await File.ReadAllTextAsync(logPath)).Trim();
        if (log.Length == 0)
        {
            StepSummary.Write("## 🚀 AI Deployment Validation\n\n> Empty deployment log — skipped.");
            return 0;
        }

        // Keep the most recent (and most relevant) portion of the log.
        const int maxChars = 20000;
        if (log.Length > maxChars) log = log.Substring(log.Length - maxChars);

        var prompt = $@"
You are an Azure DevOps Engineer reviewing a CI/CD build & deployment log.
Determine whether the deployment succeeded and surface real risks.

Return ONLY valid JSON (no markdown, no prose) in EXACTLY this shape:
{{
  ""success"": true,
  ""overallRisk"": ""Info"",
  ""failedSteps"": [],
  ""risks"": [],
  ""recommendations"": [],
  ""summary"": ""one sentence""
}}
overallRisk must be one of: Critical, High, Medium, Low, Info.
Set success=false and overallRisk=Critical if the log shows the build/publish/deploy failed.

Log:
{log}
";

        DeploymentValidation? v;
        try
        {
            var client = AzureOpenAiClient.Create();
            var raw = await AiHelpers.CompleteAsync(client, prompt);
            v = AiHelpers.ParseJson<DeploymentValidation>(raw);
        }
        catch (Exception ex)
        {
            StepSummary.Write(
                "## 🚀 AI Deployment Validation\n\n" +
                $"> ⚠️ Validation could not run ({ex.Message}). Continuing (fail-open).");
            return 0;
        }

        if (v is null)
        {
            StepSummary.Write("## 🚀 AI Deployment Validation\n\n> ⚠️ Could not parse AI response. Continuing.");
            return 0;
        }

        var sb = new StringBuilder();
        sb.AppendLine("## 🚀 AI Deployment Validation");
        sb.AppendLine();
        sb.AppendLine($"**Status:** {(v.Success ? "✅ Success" : "❌ Failed")}  |  **Risk:** {v.OverallRisk}");
        sb.AppendLine();
        if (!string.IsNullOrWhiteSpace(v.Summary)) sb.AppendLine($"> {v.Summary}").AppendLine();
        AppendList(sb, "Failed Steps", v.FailedSteps);
        AppendList(sb, "Risks", v.Risks);
        AppendList(sb, "Recommendations", v.Recommendations);

        var blocking = !v.Success || v.OverallRisk == Severity.Critical;
        sb.AppendLine();
        sb.AppendLine(blocking
            ? "### ❌ Gate failed — deployment failed or carries a critical risk."
            : "### ✅ Gate passed.");

        StepSummary.Write(sb.ToString());
        return blocking ? 1 : 0;
    }

    private static void AppendList(StringBuilder sb, string heading, List<string> items)
    {
        if (items is null || items.Count == 0) return;
        sb.AppendLine($"**{heading}:**");
        foreach (var i in items) sb.AppendLine($"- {i}");
        sb.AppendLine();
    }
}
