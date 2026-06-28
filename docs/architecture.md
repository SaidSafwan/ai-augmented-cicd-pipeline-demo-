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

## Key principle

AI is embedded in the pipeline as a CLI (`AiPipeline`) — **not** exposed as REST
endpoints. GitHub Actions invokes it at specific stages and gates the build on critical
findings. AI reports are written to the GitHub Actions **Step Summary**.

## Components

- **`Api/`** — ASP.NET Core (.NET 8) app that gets built and deployed. Exposes
  `/api/health` and Swagger. Contains no AI code.
- **`AiPipeline/`** — .NET 8 console CLI. Reads `AZURE_OPENAI_*` from environment
  variables, calls Azure OpenAI (GPT-4.1-mini), and signals pass/fail via exit codes:
  - `review` — AI code review on the diff — **gates on Critical/High**
  - `gen-tests` — suggested xUnit tests as an artifact (advisory)
  - `validate-deploy` — analyzes the build/deploy log — **gates on critical risk**
  - `analyze-incident` — root-cause analysis on pipeline failure (advisory)
- **`.github/workflows/ci.yml`** — orchestrates the stages above on push to `main`.
