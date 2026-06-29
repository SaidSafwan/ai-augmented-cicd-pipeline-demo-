# Project Summary — AI-Augmented CI/CD Pipeline

> **Purpose of this file:** a self-contained context document to paste into a new
> Claude chat. It explains what the project was, why, what we changed, and why — so
> you can then write a precise prompt for the *final* version you actually want.

---

## 1. What the project is (high level)

A demo for a conference talk — *"From Commit to Production: Building an AI-Augmented
CI/CD Pipeline"* (HackersMang, June 2026). It shows how Azure OpenAI can be woven into
a real software delivery pipeline rather than used as a standalone chatbot.

**Tech stack**

- ASP.NET Core (.NET 8) Web API
- Azure App Service (hosting) + Azure Application Insights (monitoring)
- Azure OpenAI — GPT-4.1-mini
- GitHub + GitHub Actions (CI/CD), auto-deploy on push to `main`

---

## 2. How it was originally built (the "before" state)

The AI was exposed as **REST endpoints** on the deployed Web API. A single service
(`AiEngineeringAssistantService`) wrapped Azure OpenAI and four controllers exposed it:

| Endpoint | What it did |
|---|---|
| `POST /api/review-code` | AI code review of pasted code |
| `POST /api/generate-tests` | AI-generated xUnit tests for pasted code |
| `POST /api/validate-deployment` | AI analysis of a deployment log |
| `POST /api/analyze-incident` | AI severity / root-cause / recommendation for an incident |

Plus `GET /api/health` and `GET /api/openai-check` (config probe), and Swagger UI.

**Original purpose:** demonstrate Azure OpenAI integration end-to-end (Azure deploy +
App Insights + OpenAI) by letting a user call AI features over HTTP.

**The problem with it:** because AI was behind REST endpoints, the system *felt like a
chatbot bolted onto an API*. The AI was not actually part of the engineering workflow —
a human had to manually POST code/logs to get value. It did not gate, block, or augment
the actual build-and-deploy process.

---

## 3. What we changed (the "after" state)

**Core idea:** move the AI *out* of the API and *into* the CI/CD pipeline itself, so it
runs automatically on every push and can **gate the build** on critical findings. The
Web API stops being an "AI host" and becomes just the ordinary app that the pipeline
builds and deploys.

### 3a. New `AiPipeline/` — a .NET 8 console CLI (the AI pipeline steps)

The four AI capabilities were rewritten as a command-line tool that GitHub Actions
invokes as build steps. It reads Azure OpenAI config from **environment variables**
(GitHub secrets), writes Markdown reports to the **GitHub Actions Step Summary**, and
signals pass/fail through **exit codes**.

| CLI command | Role | Gating behavior |
|---|---|---|
| `review --diff <file>` | AI code review of the pushed git diff | **exit 1 on Critical/High** (blocks deploy) |
| `gen-tests --files <a.cs,..> --out <dir>` | generates suggested xUnit tests | advisory → uploaded as build artifact |
| `validate-deploy --log <file>` | AI analysis of the build/deploy log | **exit 1 on critical risk** |
| `analyze-incident --log <file>` | root-cause analysis when pipeline fails | advisory |

Design choices: AI calls **fail open** (an OpenAI outage never blocks a deploy); large
diffs/logs are truncated to protect the model context window; all output is severity-tagged.

### 3b. Rewrote `.github/workflows/ci.yml` (push to `main`)

```
git push (main) → GitHub Actions
  ├─ Compute diff of the pushed commits
  ├─ AI Code Review          (GATE: Critical/High → fail)
  ├─ Build (Api)
  ├─ AI Test Generation      (advisory → artifact)
  ├─ Publish & Deploy ──▶ Azure App Service
  ├─ AI Deployment Validation (GATE: critical risk → fail)
  └─ AI Incident Analysis    (runs only on failure)
```

Azure OpenAI secrets flow in via job-level `env`.

### 3c. Stripped AI from the Web API (`Api/`)

- Deleted all 4 AI controllers, all AI request/response models, and
  `AiEngineeringAssistantService`.
- Removed the AI service registration and the `/api/openai-check` endpoint from `Program.cs`.
- Removed the `Azure.AI.OpenAI` NuGet package from the API project.
- Result: the API is now `GET /api/health` (via `HealthController`) + Swagger.
- Bug fix found during testing: `HealthController` and a duplicate minimal-API
  `/api/health` both matched the same route (`AmbiguousMatchException` → HTTP 500).
  Removed the duplicate; health now returns 200.

### 3e. Added demo-lever endpoints (`ProductsController`)

To make the **monitoring** half of the talk tangible, the Web API gained a
`ProductsController` with mock in-memory data and three levers for deliberately tripping
Azure Monitor alerts on stage:

- `GET /api/products` / `GET /api/products/{id}` — normal baseline requests.
- `?slow=true` — `Task.Delay(5000)` → server-response-time alert.
- `?crash=true` — uncaught `InvalidOperationException` → HTTP 500 (no swallowing
  `try/catch`, so App Insights logs a failed request) → failure-rate alert.
- `POST /api/orders` — appends to a `static` list that is never GC'd → memory-growth alert.

App Insights startup fix: the AspNetCore 3.x SDK is OpenTelemetry-based and throws at
startup when the connection-string key is present but empty. Telemetry is now registered
**only when a non-empty connection string is resolved** (`ApplicationInsights:ConnectionString`,
falling back to the `APPLICATIONINSIGHTS_CONNECTION_STRING` env var), so the empty
placeholder in `appsettings.json` lets local dev run cleanly.

### 3d. Updated docs

`README.md`, `docs/architecture.md`, `docs/demo-flow.md`, `docs/progress-log.md` were
all reframed around AI-as-pipeline-steps, with a live demo script.

---

## 4. Decisions made during the change (the "why")

These were chosen deliberately (and could be revisited in the final version):

1. **Mechanism:** a .NET CLI invoked by the workflow (vs. inline bash/curl scripts, or
   GitHub Actions calling the deployed API). Chosen because it reuses the existing C#
   Azure OpenAI code and is testable/maintainable.
2. **Gating:** advisory reports always, but **fail the build only on Critical/High**
   findings (realistic quality gate) — not strict-on-everything, not advisory-only.
3. **Trigger:** **push to `main` only** (one pipeline). PR-triggered review with inline
   PR comments was deferred.
4. **Web API fate:** stripped of AI, kept as the deployable sample app.

---

## 5. Known limitations / explicitly deferred (candidates for the final version)

- `gen-tests` output is **not compiled or run** — there's no test project yet, so the
  generated tests are just suggestions in an artifact. (A real xUnit project + a
  `dotnet test` gate is future work.)
- `analyze-incident` reads **captured pipeline logs**, not live Application Insights
  telemetry. (Querying App Insights directly is future work.)
- No PR-trigger / no inline PR comments (push-to-main only).
- No notifications (Teams/Slack), no RAG knowledge base, no auto-remediation.

---

## 6. Current repository layout (after the change)

```
Api/                       ASP.NET Core app that gets built & deployed (health + Swagger + demo levers)
  controllers/HealthController.cs
  controllers/ProductsController.cs   demo-lever endpoints (slow / crash / memory-leak)
  appsettings.json         ApplicationInsights:ConnectionString placeholder
  Api.http                 sample requests for the demo-lever endpoints
  Program.cs               conditional App Insights registration (only when configured)
  Api.csproj
AiPipeline/                .NET 8 console CLI — the AI pipeline steps
  Program.cs               arg dispatch (review | gen-tests | validate-deploy | analyze-incident)
  AzureOpenAiClient.cs     builds ChatClient from AZURE_OPENAI_* env vars
  AiHelpers.cs             model call + JSON-from-Markdown parsing
  StepSummary.cs           writes Markdown to GITHUB_STEP_SUMMARY
  Models/Finding.cs        Severity enum, Finding, DeploymentValidation, IncidentResult
  Commands/                ReviewCommand, GenTestsCommand, ValidateDeployCommand, AnalyzeIncidentCommand
.github/workflows/ci.yml   the pipeline wiring
docs/                      architecture.md, demo-flow.md, progress-log.md, PROJECT-SUMMARY.md
README.md
```

**Required GitHub repo secrets:** `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`,
`AZURE_OPENAI_DEPLOYMENT_NAME`, `AZURE_WEBAPP_PUBLISH_PROFILE`.

---

## 7. How to use this document

Paste sections 1–6 into a new Claude chat as background. Then describe the **final**
project you actually want — for example, by answering:

- Should AI **gate** the build, only advise, or both? At what severity threshold?
- **PRs** (with inline comments) or **push-to-main** only — or both?
- Do you want a **real test project** that gen-tests writes into and `dotnet test` runs?
- Should incident analysis pull from **Application Insights** live, or stay log-based?
- Keep the AI logic as a **CLI**, or also re-expose some of it as an API/MCP server?
- Any **notifications** (Teams/Slack/email) or a **dashboard**?
- Is the deployed `Api/` still meaningful, or should the repo become *only* the
  AI pipeline tooling?

Claude can then turn your answers into a precise build prompt for the final system.
