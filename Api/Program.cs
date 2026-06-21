using Api.services;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Register Service
builder.Services.AddSingleton<CodeReviewService>();
builder.Services.AddSingleton<IncidentAnalysisService>();

builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure pipeline
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/api/openai-check", (IConfiguration config) =>
{
    return Results.Ok(new
    {
        EndpointExists = !string.IsNullOrEmpty(config["AZURE_OPENAI_ENDPOINT"]),
        KeyExists = !string.IsNullOrEmpty(config["AZURE_OPENAI_API_KEY"]),
        DeploymentExists = !string.IsNullOrEmpty(config["AZURE_OPENAI_DEPLOYMENT"])
    });
});
app.Run();