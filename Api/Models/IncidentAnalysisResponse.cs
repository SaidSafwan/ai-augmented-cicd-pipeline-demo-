namespace Api.Models
{
    public class IncidentAnalysisResponse
    {
        public string Severity { get; set; } = string.Empty;
        public string RootCause { get; set; } = string.Empty;
        public string Recommendation { get; set;} = string.Empty;
    }
}
