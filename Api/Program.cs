var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Only wire up Application Insights when a connection string is actually present.
// The AspNetCore 3.x SDK is OpenTelemetry-based and throws at startup if the
// connection string is configured but empty. Read the config key first, then
// fall back to the APPLICATIONINSIGHTS_CONNECTION_STRING env var.
var appInsightsConnectionString =
    builder.Configuration["ApplicationInsights:ConnectionString"];
if (string.IsNullOrWhiteSpace(appInsightsConnectionString))
{
    appInsightsConnectionString =
        builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
}

if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
    });
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.MapControllers();

// Health is served by HealthController (GET /api/health). The previous duplicate
// minimal-API endpoint on the same route caused an AmbiguousMatchException.

app.Run();