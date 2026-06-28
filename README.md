# AI-Augmented CI/CD Pipeline Demo

Demo project created for my HackersMang June 2026 session:

## Session

**From Commit to Production: Building an AI-Augmented CI/CD Pipeline**

---

## Overview

This project demonstrates a modern DevOps workflow that combines:

- Continuous Integration (CI)
- Continuous Deployment (CD)
- Cloud Hosting
- Production Monitoring
- **AI embedded in the pipeline** (not exposed as REST endpoints)

AI is **not** a chatbot API. It runs as automated steps **inside GitHub Actions**:
on every push to `main`, the `AiPipeline` CLI reviews the diff, generates tests,
validates the deployment, and analyzes incidents on failure — **gating the build on
critical findings**. The app itself is a plain ASP.NET Core service that gets built,
deployed to Azure, and monitored through Application Insights.

---

## Architecture

```text
Developer
    │  git push (main)
    ▼
GitHub Actions
    │
    ├─▶ AI Code Review ........... gates on Critical/High  ─┐
    ├─▶ Build                                               │ AiPipeline CLI
    ├─▶ AI Test Generation ....... advisory artifact        │      +
    ├─▶ Publish & Deploy ─▶ Azure App Service               │ Azure OpenAI
    ├─▶ AI Deployment Validation . gates on critical risk   │ GPT-4.1-mini
    └─▶ AI Incident Analysis ..... runs on failure  ───────┘
                    │
                    ▼
        Reports written to the GitHub Actions Step Summary
                    │
                    ▼
        Azure App Service  ─▶  Application Insights (monitoring)
```

---

## Objectives

- Automated CI/CD with GitHub Actions
- AI code review as a pipeline quality gate
- AI test generation in CI
- AI deployment validation gate
- AI incident analysis on pipeline failure
- Azure App Service Deployment + Application Insights monitoring

## Required GitHub Secrets

| Secret | Purpose |
|---|---|
| `AZURE_OPENAI_ENDPOINT` | Azure OpenAI resource endpoint |
| `AZURE_OPENAI_API_KEY` | Azure OpenAI API key |
| `AZURE_OPENAI_DEPLOYMENT_NAME` | GPT-4.1-mini deployment name |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | Azure App Service publish profile |

---

## Features

### CI/CD Automation

- GitHub Actions Workflow
- Automated Build Validation
- Automated Deployment
- Production Release Pipeline

### Cloud Deployment

- Azure App Service Hosting
- Production Environment Deployment
- Health Check Endpoint

### Monitoring & Observability

- Application Insights Integration
- Request Tracking
- Exception Monitoring
- Performance Metrics
- Log Collection

### AI Pipeline Steps (`AiPipeline` CLI)

The AI runs as a console tool the workflow invokes — no REST endpoints. Each command
writes a Markdown report to the GitHub Actions Step Summary and uses its exit code to
gate the build.

| Command | Role | Gate |
|---|---|---|
| `review --diff <file>` | Reviews the pushed diff for bugs / security / perf / smells | Fails build on **Critical/High** |
| `gen-tests --files <a.cs,..> --out <dir>` | Generates suggested xUnit tests | Advisory (artifact) |
| `validate-deploy --log <file>` | Validates the build/publish/deploy log | Fails build on **critical risk** |
| `analyze-incident --log <file>` | Root-cause analysis when the pipeline fails | Advisory |

Run locally (set the `AZURE_OPENAI_*` env vars first):

```bash
git diff HEAD~1 HEAD > diff.patch
dotnet run --project AiPipeline -- review --diff diff.patch
```

Example incident-analysis output (written to the Step Summary):

```json
{
  "severity": "High",
  "rootCause": "Application attempted to access a null object reference.",
  "recommendation": "Add null validation and improve exception handling."
}
```

---

## Tech Stack

### Backend

- ASP.NET Core (.NET 8) — deployable app (`/api/health` + Swagger)
- `AiPipeline` — .NET 8 console CLI (the AI pipeline steps)
- C#

### DevOps

- GitHub Actions
- GitHub Repository

### Cloud

- Azure App Service
- Azure Resource Group

### Monitoring

- Application Insights

### AI

- Azure OpenAI
- GPT-4.1-mini

---

## Project Status

### Completed

- GitHub Repository Setup
- ASP.NET Core Web API (health-only deployable app)
- Azure Resource Group / App Service / Application Insights
- GitHub Actions CI/CD with Automated Azure Deployment
- Azure OpenAI Integration
- `AiPipeline` CLI: AI Code Review, Test Generation, Deployment Validation, Incident Analysis
- AI code review + deployment validation quality gates

---

## Future Enhancements

- PR-triggered AI review with inline PR comments
- Real xUnit test project + `dotnet test` gate (currently gen-tests is advisory)
- Pull incident logs directly from Application Insights
- Automated Remediation Suggestions
- Teams/Slack Notifications
- RAG-Based Incident Knowledge Base

---

## Demo Session

**From Commit to Production: Building an AI-Augmented CI/CD Pipeline**

HackersMang June 2026 Edition
