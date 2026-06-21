using Api.services;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Register Service
builder.Services.AddSingleton<CodeReviewService>();

var app = builder.Build();

// Configure pipeline
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();