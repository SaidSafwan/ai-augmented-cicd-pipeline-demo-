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

## Monitoring demo — triggering Azure Monitor alerts live

The deployed Web API ships **demo-lever endpoints** (`ProductsController`) so you can make
Application Insights light up on demand during the talk:

4. **Slow request** — hit `GET /api/products/1?slow=true` (a 5s `Task.Delay`). Watch the
   server-response-time spike and the corresponding Azure Monitor alert fire.
5. **Failed request** — hit `GET /api/products/1?crash=true`. The endpoint throws an
   uncaught exception → HTTP 500, which App Insights records as a failed request (and a
   failure-rate alert can fire).
6. **Memory growth** — POST repeatedly to `/api/orders`; each order is appended to a
   `static` list that never gets garbage-collected, so process memory climbs.

```powershell
# baseline
curl https://<app>.azurewebsites.net/api/products
# slow lever (~5s)
curl "https://<app>.azurewebsites.net/api/products/1?slow=true"
# crash lever (HTTP 500)
curl -i "https://<app>.azurewebsites.net/api/products/1?crash=true"
# memory-leak lever (run a few times)
curl -X POST https://<app>.azurewebsites.net/api/orders -H "Content-Type: application/json" -d '{"productId":1,"quantity":2}'
```

> App Insights only activates when a connection string is configured
> (`ApplicationInsights:ConnectionString`, or the `APPLICATIONINSIGHTS_CONNECTION_STRING`
> env var). Locally, with the empty placeholder, telemetry stays off and the app runs fine.

## Run a step locally

```bash
# requires AZURE_OPENAI_ENDPOINT / AZURE_OPENAI_API_KEY / AZURE_OPENAI_DEPLOYMENT_NAME
git diff HEAD~1 HEAD > diff.patch
dotnet run --project AiPipeline -- review --diff diff.patch
```
