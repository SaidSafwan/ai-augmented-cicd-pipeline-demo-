# Architecture

```text
┌─────────────────────┐
│      Developer      │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ GitHub Repository   │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  GitHub Actions     │
├─────────────────────┤
│ • AI Code Review    │
│ • Build Validation  │
│ • Automated Testing │
│ • Deployment Pipe.  │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Azure App Service   │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│Application Insights │
├─────────────────────┤
│ • Requests          │
│ • Exceptions        │
│ • Performance       │
│ • Application Logs  │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ AI Incident Analysis│
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Recommendations &   │
│ Root Cause Analysis │
└─────────────────────┘
```
