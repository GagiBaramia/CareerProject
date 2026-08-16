using CareerProject.RecommendationService.Consumers;
using CareerProject.RecommendationService.Services;
using CareerProject.Shared.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<CareerProjectDbContext>(options =>
    options.UseNpgsql(PostgresConnectionStringBuilder.BuildFromEnvironment(), o => o.UseVector()));

builder.Services.AddHttpClient<GeminiEmbeddingClient>();
builder.Services.AddScoped<PersonProfileEmbeddingService>();
builder.Services.AddScoped<JobEmbeddingService>();

// Embedding is a background side effect of profile/job saves in other
// services - these consumers never block the original HTTP request.
builder.Services.AddHostedService<ProfileCreatedConsumer>();
builder.Services.AddHostedService<ProfileUpdatedConsumer>();
builder.Services.AddHostedService<JobCreatedConsumer>();
builder.Services.AddHostedService<JobUpdatedConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
