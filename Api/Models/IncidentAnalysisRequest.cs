using System.Globalization;

namespace Api.Models
{
    public class IncidentAnalysisRequest
    {
        public string Error { get; set; } = string.Empty;
        public string Logs { get; set; } = string.Empty;
    }
}
