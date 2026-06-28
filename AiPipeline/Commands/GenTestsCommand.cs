using System.Text;

namespace AiPipeline.Commands;

/// <summary>
/// `gen-tests --files &lt;a.cs,b.cs&gt; --out &lt;dir&gt;` — generates suggested xUnit tests
/// for changed C# files. Advisory only (always exit 0): the tests are written to the
/// output dir as a build artifact and are NOT compiled/run, since generated code isn't
/// guaranteed to compile.
/// </summary>
public static class GenTestsCommand
{
    public static async Task<int> RunAsync(IReadOnlyDictionary<string, string> args)
    {
        var outDir = args.GetValueOrDefault("out") ?? "generated-tests";
        var fileList = args.GetValueOrDefault("files") ?? "";

        var files = fileList
            .Split(new[] { ',', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                        && !f.EndsWith("Tests.cs", StringComparison.OrdinalIgnoreCase)
                        && File.Exists(f))
            .Distinct()
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("## 🧪 AI Test Generation");
        sb.AppendLine();

        if (files.Count == 0)
        {
            sb.AppendLine("> No changed C# source files to generate tests for.");
            StepSummary.Write(sb.ToString());
            return 0;
        }

        Directory.CreateDirectory(outDir);

        var generated = 0;
        foreach (var file in files)
        {
            string code;
            try { code = await File.ReadAllTextAsync(file); }
            catch { continue; }

            if (string.IsNullOrWhiteSpace(code)) continue;

            var prompt = $@"
Generate production-ready xUnit test cases for the following C# code.
Cover the meaningful behavior and edge cases. Return ONLY C# code (no markdown,
no explanation). Include the necessary using statements.

Source file: {Path.GetFileName(file)}

{code}
";

            try
            {
                var client = AzureOpenAiClient.Create();
                var raw = await AiHelpers.CompleteAsync(client, prompt);
                var tests = raw.Replace("```csharp", "").Replace("```cs", "").Replace("```", "").Trim();

                var testFileName = Path.GetFileNameWithoutExtension(file) + "Tests.cs";
                var outPath = Path.Combine(outDir, testFileName);
                await File.WriteAllTextAsync(outPath, tests);
                generated++;
                sb.AppendLine($"- `{file}` → `{outPath}`");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"- `{file}` → ⚠️ skipped ({ex.Message})");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"Generated **{generated}** test file(s) into `{outDir}/` (uploaded as a build artifact).");
        sb.AppendLine("> These are AI suggestions — review before adding to the test suite.");
        StepSummary.Write(sb.ToString());
        return 0;
    }
}
