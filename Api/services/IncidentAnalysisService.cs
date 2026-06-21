using Api.Models;

namespace Api.services
{
    public class IncidentAnalysisService
    {
        public IncidentAnalysisResponse Analyze(IncidentAnalysisRequest request) {
            return new IncidentAnalysisResponse
            {
                Severity = "High",
                RootCause = "Possible null reference execption",
                Recommendation = "Add null checks and validate object initialization"
            };
        }
    }
}
