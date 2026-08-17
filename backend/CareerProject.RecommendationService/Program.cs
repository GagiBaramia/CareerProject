using System.Text;
using CareerProject.RecommendationService.Config;
using CareerProject.RecommendationService.Consumers;
using CareerProject.RecommendationService.Endpoints;
using CareerProject.RecommendationService.Services;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<CareerProjectDbContext>(options =>
    options.UseNpgsql(PostgresConnectionStringBuilder.BuildFromEnvironment(), o => o.UseVector()));

builder.Services.AddHttpClient<GeminiEmbeddingClient>();
builder.Services.AddHttpClient<GeminiChatClient>();
builder.Services.AddScoped<PersonProfileEmbeddingService>();
builder.Services.AddScoped<JobEmbeddingService>();
builder.Services.AddScoped<AiChatService>();

// Embedding is a background side effect of profile/job saves in other
// services - these consumers never block the original HTTP request.
builder.Services.AddHostedService<ProfileCreatedConsumer>();
builder.Services.AddHostedService<ProfileUpdatedConsumer>();
builder.Services.AddHostedService<JobCreatedConsumer>();
builder.Services.AddHostedService<JobUpdatedConsumer>();

builder.Services.Configure<RecommendationOptions>(builder.Configuration.GetSection(RecommendationOptions.SectionName));
builder.Services.Configure<AiChatOptions>(builder.Configuration.GetSection(AiChatOptions.SectionName));

// Validates tokens issued by CareerProject.UserService - Issuer/Audience/secret must match.
var jwtSecret = Environment.GetEnvironmentVariable("Jwt__Secret")
    ?? throw new InvalidOperationException("Jwt__Secret environment variable is not set.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PersonOnly", policy => policy.RequireRole(UserRole.Person.ToString()));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapRecommendationEndpoints();
app.MapAiChatEndpoints();

app.Run();
