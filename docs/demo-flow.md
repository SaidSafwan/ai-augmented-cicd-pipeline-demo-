# Demo Flow

The AI runs as steps **inside** the pipeline (the `AiPipeline` CLI), not as an API.

```text
┌─────────────────────┐
│      Developer      │
└──────────┬──────────┘
           │  git push (main)
           ▼
┌─────────────────────┐
│   GitHub Actions    │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  AI Code Review     │  gate: Critical/High → fail
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  Build (Api)        │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ AI Test Generation  │  advisory → artifact
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  Publish & Deploy   │ ─────▶ Azure App Service ─▶ Application Insights
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ AI Deployment       │  gate: critical risk → fail
│ Validation          │
└──────────┬──────────┘
           │  (on failure)
           ▼
┌─────────────────────┐
│ AI Incident Analysis│  advisory → root cause + recommendation
└─────────────────────┘
```

## Live demo script

1. **Happy path** — push a small, clean change to `main`. Watch the Action: AI Code
   Review passes, build/publish/deploy succeed, and the **Summary** tab shows the review,
   generated-tests, and deployment-validation reports.
2. **Quality gate** — push a commit with an obvious bug or hardcoded secret. AI Code
   Review reports a Critical/High finding and **fails the build before deploy**.
3. **Incident analysis** — force a failure (e.g. break the build or deploy step). The
   `AI Incident Analysis` step runs on failure and posts a severity / root-cause /
   recommendation summary.

## Run a step locally

```bash
# requires AZURE_OPENAI_ENDPOINT / AZURE_OPENAI_API_KEY / AZURE_OPENAI_DEPLOYMENT_NAME
git diff HEAD~1 HEAD > diff.patch
dotnet run --project AiPipeline -- review --diff diff.patch
```
