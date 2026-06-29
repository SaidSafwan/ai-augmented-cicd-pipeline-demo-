# Progress Log

## Day 1 - Project Setup

### Completed

- Created GitHub repository
- Initialized ASP.NET Core Web API
- Added project architecture documentation
- Created Azure Free Account
- Created Azure Resource Group
- Created Azure App Service
- Enabled Application Insights

---

## Day 2 - Deployment

### Completed

- Added Health Check endpoint
- Configured .gitignore
- Cleaned repository structure
- Published ASP.NET Core Web API to Azure App Service
- Verified production deployment
- Verified Swagger UI in Azure
- Verified API endpoint execution through Azure Log Stream

---

## Day 3 - CI/CD Automation

### Completed

- Configured GitHub Actions workflow
- Implemented Continuous Integration (CI)
- Implemented Continuous Deployment (CD)
- Connected GitHub repository with Azure App Service
- Verified automated deployment after code push
- Validated successful production deployment

---

## Day 4 - Monitoring & Observability

### Completed

- Configured Application Insights telemetry
- Verified request tracking
- Verified application logging
- Verified exception monitoring
- Verified performance metrics collection
- Tested production monitoring capabilities

---

## Day 5 - AI Incident Analysis

### Completed

- Created Incident Analysis API endpoint
- Integrated Azure OpenAI GPT-4.1-mini
- Configured Azure OpenAI deployment
- Implemented AI-powered incident analysis service
- Generated severity classification
- Generated root cause analysis
- Generated remediation recommendations
- Verified end-to-end AI workflow

---

## Day 6 - AI Moved Into the Pipeline (2026-06-26)

### Completed

- Removed all AI REST endpoints from the Web API (no more chatbot-style API)
- Built `AiPipeline`, a .NET 8 console CLI with four subcommands:
  `review`, `gen-tests`, `validate-deploy`, `analyze-incident`
- AI now runs as steps inside GitHub Actions on every push to `main`
- AI Code Review runs on the commit diff and **gates** the build (fails on Critical/High)
- AI Deployment Validation **gates** on critical deployment risk
- AI Test Generation produces suggested xUnit tests as a build artifact (advisory)
- AI Incident Analysis runs automatically when the pipeline fails
- All AI output is written to the GitHub Actions **Step Summary**
- Web API reduced to the deployable sample app (`/api/health` + Swagger)

---

## Day 7 - Monitoring Demo Levers (2026-06-28)

### Completed

- Added `ProductsController` to the Web API with mock in-memory products/orders
- Added three **demo-lever endpoints** to deliberately trigger Azure Monitor alerts live:
  - `?slow=true` → 5s `Task.Delay` (server-response-time alert)
  - `?crash=true` → uncaught exception → HTTP 500 (failed-request alert)
  - `POST /api/orders` → appends to a `static` list (memory-growth alert)
- Crash lever intentionally lets the exception bubble up so App Insights logs the failure
- Added `ApplicationInsights:ConnectionString` placeholder to `appsettings.json`
- Fixed startup crash: the AspNetCore 3.x (OpenTelemetry-based) SDK threw on an empty
  connection string — telemetry is now registered only when a connection string is present
  (config key, falling back to `APPLICATIONINSIGHTS_CONNECTION_STRING`)
- Updated `Api.http` with sample requests for all new endpoints (normal/slow/crash/order)

---

## Project Status

### Completed Components

- GitHub Repository
- ASP.NET Core Web API
- GitHub Actions CI/CD Pipeline
- Azure App Service
- Application Insights
- Azure OpenAI Integration
- AiPipeline CLI (AI as pipeline steps)
- AI Code Review quality gate
- AI Deployment Validation quality gate
- AI Test Generation (artifact)
- AI Incident Analysis (on failure)
- Demo-lever endpoints (slow / crash / memory-leak) for live Azure Monitor alerts
- Production Deployment
- Automated Monitoring

---

## Demo Scenario

### Input

```json
{
  "error": "500",
  "logs": "NullReferenceException"
}
```

### Output

- Severity Analysis
- Root Cause Detection
- Recommendation Generation

---

## Key Achievements

- Automated CI/CD using GitHub Actions
- Cloud Deployment using Azure App Service
- Production Monitoring using Application Insights
- AI-Powered Incident Analysis using Azure OpenAI
- End-to-End Production Workflow Demonstration

---

## Future Enhancements

- AI-Powered Pull Request Review
- Automated Code Quality Analysis
- Teams/Slack Incident Notifications
- RAG-Based Incident Knowledge Base
- AI-Generated Remediation Scripts
- Automated Alert Analysis
- Production Incident Dashboard
