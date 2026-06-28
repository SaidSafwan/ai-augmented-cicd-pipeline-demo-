using System.Text;
using AiPipeline.Models;

namespace AiPipeline.Commands;

/// <summary>
/// `review --diff &lt;file&gt;` — AI code review over a unified git diff.
/// Writes a Markdown findings table to the Step Summary and GATES the pipeline:
/// exit 1 when any Critical/High finding is reported, else exit 0.
/// Fails open (exit 0) on AI/transport error so an OpenAI outage can't block deploys.
/// </summary>
public static class ReviewCommand
{
    public static async Task<int> RunAsync(IReadOnlyDictionary<string, string> args)
    {
        var diffPath = args.GetValueOrDefault("diff");
        if (string.IsNullOrWhiteSpace(diffPath) || !File.Exists(diffPath))
        {
            StepSummary.Write("## 🤖 AI Code Review\n\n> No diff file provided — skipped.");
            return 0;
        }

        var diff = (await File.ReadAllTextAsync(diffPath)).Trim();
        if (diff.Length == 0)
        {
            StepSummary.Write("## 🤖 AI Code Review\n\n> No code changes to review.");
            return 0;
        }

        // Guard against huge diffs blowing the context window.
        const int maxChars = 24000;
        var truncated = diff.Length > maxChars;
        if (truncated) diff = diff.Substring(0, maxChars);

        var prompt = $@"
You are a Senior Software Engineer doing a pull-request review.
Review ONLY the changes in the unified git diff below.

For each issue you find, classify it. Use these categories:
Bug, Security, Performance, CodeSmell, BestPractice.
Use these severities: Critical, High, Medium, Low, Info.
Be strict about real Bugs and Security issues; do not invent problems.

Return ONLY valid JSON (no markdown, no prose) in EXACTLY this shape:
{{
  ""findings"": [
    {{ ""severity"": ""High"", ""category"": ""Security"", ""title"": ""..."", ""detail"": ""..."", ""location"": ""file.cs"" }}
  ]
}}
If there are no issues, return {{ ""findings"": [] }}.

Diff:
{diff}
";

        ReviewResult? result;
        try
        {
            var client = AzureOpenAiClient.Create();
            var raw = await AiHelpers.CompleteAsync(client, prompt);
            result = AiHelpers.ParseJson<ReviewResult>(raw);
        }
        catch (Exception ex)
        {
            StepSummary.Write(
                "## 🤖 AI Code Review\n\n" +
                $"> ⚠️ AI review could not run ({ex.Message}). Continuing (fail-open).");
            return 0;
        }

        var findings = result?.Findings ?? new List<Finding>();
        var sb = new StringBuilder();
        sb.AppendLine("## 🤖 AI Code Review");
        sb.AppendLine();

        if (findings.Count == 0)
        {
            sb.AppendLine("✅ No issues found in the changed code.");
            StepSummary.Write(sb.ToString());
            return 0;
        }

        findings.Sort((a, b) => b.Severity.CompareTo(a.Severity));

        sb.AppendLine("| Severity | Category | Title | Location |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var f in findings)
        {
            sb.AppendLine($"| {Badge(f.Severity)} | {f.Category} | {Escape(f.Title)} | {Escape(f.Location ?? "-")} |");
        }
        sb.AppendLine();
        foreach (var f in findings)
        {
            sb.AppendLine($"- **{f.Severity} · {f.Category} — {Escape(f.Title)}**: {Escape(f.Detail)}");
        }
        if (truncated)
        {
            sb.AppendLine();
            sb.AppendLine("> ℹ️ Diff was large and truncated for review.");
        }

        var blocking = findings.Count(f => f.Severity >= Severity.High);
        sb.AppendLine();
        if (blocking > 0)
        {
            sb.AppendLine($"### ❌ Gate failed — {blocking} Critical/High finding(s) must be addressed.");
            StepSummary.Write(sb.ToString());
            return 1;
        }

        sb.AppendLine("### ✅ Gate passed — only non-blocking findings.");
        StepSummary.Write(sb.ToString());
        return 0;
    }

    private static string Badge(Severity s) => s switch
    {
        Severity.Critical => "🟥 Critical",
        Severity.High => "🟧 High",
        Severity.Medium => "🟨 Medium",
        Severity.Low => "🟦 Low",
        _ => "⬜ Info"
    };

    private static string Escape(string s) =>
        s.Replace("|", "\\|").Replace("\n", " ").Trim();

    private sealed class ReviewResult
    {
        public List<Finding> Findings { get; set; } = new();
    }
}
