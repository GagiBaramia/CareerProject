using System.Text;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using CareerProject.Shared.Messaging;
using CareerProject.Shared.Storage;
using CareerProject.UserService.Auth;
using CareerProject.UserService.Config;
using CareerProject.UserService.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// UseStaticFiles() builds its file provider from wwwroot at WebApplicationBuilder construction
// time - the folder must exist on disk before CreateBuilder runs, not merely before Build().
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos"));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<CareerProjectDbContext>(options =>
    options.UseNpgsql(PostgresConnectionStringBuilder.BuildFromEnvironment(), o => o.UseVector()));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<JwtTokenService>();

builder.Services.Configure<UploadOptions>(builder.Configuration.GetSection(UploadOptions.SectionName));
builder.Services.AddSingleton<IFileStorage>(_ =>
    new LocalFileStorage(Path.Combine(builder.Environment.ContentRootPath, "wwwroot")));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PersonOnly", policy => policy.RequireRole(UserRole.Person.ToString()));
});

builder.Services.AddCareerProjectEventPublisher();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CareerProject.UserService v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapSkillsEndpoints();
app.MapProfileEndpoints();

app.Run();

// Needed so CareerProject.UserService.Tests can reference this entry point via WebApplicationFactory<Program>.
public partial class Program { }
