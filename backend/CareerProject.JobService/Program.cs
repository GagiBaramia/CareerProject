using System.Text;
using CareerProject.JobService.Endpoints;
using CareerProject.JobService.Events;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<CareerProjectDbContext>(options =>
    options.UseNpgsql(PostgresConnectionStringBuilder.BuildFromEnvironment(), o => o.UseVector()));

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
    options.AddPolicy("CompanyOnly", policy => policy.RequireRole(UserRole.Company.ToString()));
});

builder.Services.AddSingleton<JobEventPublisher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapCompanyEndpoints();
app.MapJobEndpoints();

app.Run();
