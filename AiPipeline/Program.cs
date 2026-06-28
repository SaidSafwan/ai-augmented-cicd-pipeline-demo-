using AiPipeline.Commands;

// AiPipeline — the AI steps of the CI/CD pipeline as a CLI.
//
// Usage:
//   AiPipeline review           --diff <file>
//   AiPipeline gen-tests        --files <a.cs,b.cs> --out <dir>
//   AiPipeline validate-deploy  --log <file>
//   AiPipeline analyze-incident --log <file> [--error <text>]
//
// Config (env vars / GitHub secrets):
//   AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, AZURE_OPENAI_DEPLOYMENT_NAME
//
// Exit codes: 0 = ok / advisory, 1 = quality gate failed, 2 = bad usage.

if (args.Length == 0)
{
    PrintUsage();
    return 2;
}

var command = args[0].ToLowerInvariant();
var options = ParseOptions(args.Skip(1));

try
{
    return command switch
    {
        "review" => await ReviewCommand.RunAsync(options),
        "gen-tests" => await GenTestsCommand.RunAsync(options),
        "validate-deploy" => await ValidateDeployCommand.RunAsync(options),
        "analyze-incident" => await AnalyzeIncidentCommand.RunAsync(options),
        _ => Unknown(command)
    };
}
catch (Exception ex)
{
    // Last-resort guard: never let an unexpected crash take down the pipeline silently.
    Console.Error.WriteLine($"AiPipeline '{command}' errored: {ex.Message}");
    return 0;
}

static int Unknown(string command)
{
    Console.Error.WriteLine($"Unknown command: '{command}'");
    PrintUsage();
    return 2;
}

// Parse "--flag value" pairs (and bare "--flag" booleans → "true").
static Dictionary<string, string> ParseOptions(IEnumerable<string> rest)
{
    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    var list = rest.ToList();
    for (var i = 0; i < list.Count; i++)
    {
        var token = list[i];
        if (!token.StartsWith("--")) continue;
        var key = token[2..];
        if (i + 1 < list.Count && !list[i + 1].StartsWith("--"))
        {
            dict[key] = list[++i];
        }
        else
        {
            dict[key] = "true";
        }
    }
    return dict;
}

static void PrintUsage()
{
    Console.WriteLine(
        "AiPipeline — AI steps for the CI/CD pipeline\n\n" +
        "Commands:\n" +
        "  review           --diff <file>\n" +
        "  gen-tests        --files <a.cs,b.cs> --out <dir>\n" +
        "  validate-deploy  --log <file>\n" +
        "  analyze-incident --log <file> [--error <text>]\n");
}
