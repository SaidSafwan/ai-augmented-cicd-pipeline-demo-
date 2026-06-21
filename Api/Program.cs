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

app.MapGet("/api/openai-config", (IConfiguration config) =>
{
    return Results.Ok(new
    {
        Endpoint = config["AZURE_OPENAI_ENDPOINT"],
        Deployment = config["AZURE_OPENAI_DEPLOYMENT_NAME"]
    });
});
app.Run();