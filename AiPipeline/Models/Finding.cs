using System.Text.Json.Serialization;

namespace AiPipeline.Models;

public enum Severity
{
    Info,
    Low,
    Medium,
    High,
    Critical
}

/// <summary>A single AI finding from a code review or deployment validation.</summary>
public sealed class Finding
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Severity Severity { get; set; } = Severity.Info;

    /// <summary>Bug, Security, Performance, CodeSmell, BestPractice, Risk, etc.</summary>
    public string Category { get; set; } = "General";

    public string Title { get; set; } = "";

    public string Detail { get; set; } = "";

    /// <summary>Optional file / location hint.</summary>
    public string? Location { get; set; }
}

/// <summary>Structured result of AI deployment-log validation.</summary>
public sealed class DeploymentValidation
{
    public bool Success { get; set; } = true;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Severity OverallRisk { get; set; } = Severity.Info;

    public List<string> FailedSteps { get; set; } = new();

    public List<string> Risks { get; set; } = new();

    public List<string> Recommendations { get; set; } = new();

    public string Summary { get; set; } = "";
}

/// <summary>Structured incident analysis (mirrors the old IncidentAnalysisResponse).</summary>
public sealed class IncidentResult
{
    public string Severity { get; set; } = "Unknown";
    public string RootCause { get; set; } = "";
    public string Recommendation { get; set; } = "";
}
