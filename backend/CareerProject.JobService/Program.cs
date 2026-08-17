using System.Text;
using CareerProject.JobService.Config;
using CareerProject.JobService.Endpoints;
using CareerProject.JobService.Hubs;
using CareerProject.JobService.Services;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using CareerProject.Shared.Messaging;
using CareerProject.Shared.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// UseStaticFiles() builds its file provider from wwwroot at WebApplicationBuilder construction
// time - the folder must exist on disk before CreateBuilder runs, not merely before Build().
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos"));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<CareerProjectDbContext>(options =>
    options.UseNpgsql(PostgresConnectionStringBuilder.BuildFromEnvironment(), o => o.UseVector()));

builder.Services.AddScoped<ConversationService>();
builder.Services.AddSignalR();

builder.Services.Configure<UploadOptions>(builder.Configuration.GetSection(UploadOptions.SectionName));
builder.Services.AddSingleton<IFileStorage>(_ =>
    new LocalFileStorage(Path.Combine(builder.Environment.ContentRootPath, "wwwroot")));

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

        // Browsers can't set Authorization headers on WebSocket upgrades - SignalR's documented
        // workaround is reading the token from the query string for requests to the hub path.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hub"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CompanyOnly", policy => policy.RequireRole(UserRole.Company.ToString()));
    options.AddPolicy("PersonOnly", policy => policy.RequireRole(UserRole.Person.ToString()));
});

builder.Services.AddCareerProjectEventPublisher();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapCompanyEndpoints();
app.MapJobEndpoints();
app.MapApplicationEndpoints();
app.MapConversationEndpoints();
app.MapHub<ChatHub>("/hub/chat");

app.Run();

// Needed so CareerProject.JobService.Tests can reference this entry point via WebApplicationFactory<Program>.
public partial class Program { }
