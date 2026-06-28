namespace AiPipeline;

/// <summary>
/// Appends Markdown to the GitHub Actions Step Summary (the file path lives in
/// the GITHUB_STEP_SUMMARY env var). When running locally that var is absent, so
/// output falls back to the console.
/// </summary>
public static class StepSummary
{
    private static readonly string? SummaryPath =
        Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");

    public static void Write(string markdown)
    {
        if (!string.IsNullOrWhiteSpace(SummaryPath))
        {
            File.AppendAllText(SummaryPath, markdown + Environment.NewLine);
        }

        // Always echo to the console so logs and local runs show the report too.
        Console.WriteLine(markdown);
    }
}
