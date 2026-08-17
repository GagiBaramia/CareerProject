using System.Security.Claims;
using CareerProject.JobService.Auth;
using CareerProject.JobService.Dtos;
using CareerProject.JobService.Services;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using CareerProject.Shared.Events;
using CareerProject.Shared.Messaging;
using CareerProject.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace CareerProject.JobService.Endpoints;

public static class ApplicationEndpoints
{
    public static void MapApplicationEndpoints(this WebApplication app)
    {
        app.MapPost("/api/jobs/{jobId:guid}/apply", SubmitApplication)
            .WithTags("Applications")
            .RequireAuthorization("PersonOnly");

        app.MapGet("/api/company/jobs/{jobId:guid}/applications", ListApplicationsForJob)
            .WithTags("Applications")
            .RequireAuthorization("CompanyOnly");

        app.MapPatch("/api/applications/{id:guid}/status", UpdateApplicationStatus)
            .WithTags("Applications")
            .RequireAuthorization("CompanyOnly");
    }

    private static async Task<IResult> SubmitApplication(Guid jobId, ClaimsPrincipal user, CareerProjectDbContext db, IEventPublisher publisher)
    {
        var profile = await CurrentUserResolver.LoadCurrentPersonProfileAsync(user, db);
        if (profile is null)
            return Results.NotFound(new { message = "Person profile not found." });

        var jobExists = await db.Jobs.AnyAsync(j => j.Id == jobId);
        if (!jobExists)
            return Results.NotFound(new { message = "Job not found." });

        var jobIdsAlreadyAppliedTo = await db.Applications
            .Where(a => a.PersonId == profile.Id)
            .Select(a => a.JobId)
            .ToListAsync();

        if (ApplicationRules.IsDuplicate(jobIdsAlreadyAppliedTo, jobId))
            return Results.Conflict(new { message = "You have already applied to this job." });

        var application = new Application
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            PersonId = profile.Id,
            Status = ApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow,
        };

        db.Applications.Add(application);
        await db.SaveChangesAsync();
        await publisher.PublishAsync(new ApplicationSubmitted { EntityId = application.Id });

        var created = await LoadApplication(db, application.Id);
        return Results.Created($"/api/applications/{application.Id}", ToResponse(created!));
    }

    private static async Task<IResult> ListApplicationsForJob(Guid jobId, ClaimsPrincipal user, CareerProjectDbContext db)
    {
        var company = await CurrentUserResolver.LoadCurrentCompanyAsync(user, db);
        if (company is null)
            return Results.NotFound(new { message = "Company profile not found." });

        var job = await db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job is null)
            return Results.NotFound();

        if (job.CompanyId != company.Id)
            return Results.Forbid();

        var applications = await db.Applications
            .Include(a => a.Job)
            .Include(a => a.Person)
            .Where(a => a.JobId == jobId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return Results.Ok(applications.Select(ToResponse));
    }

    private static async Task<IResult> UpdateApplicationStatus(
        Guid id,
        UpdateApplicationStatusRequest request,
        ClaimsPrincipal user,
        CareerProjectDbContext db,
        IEventPublisher publisher)
    {
        if (!RequestValidator.TryValidate(request, out var errors))
            return Results.ValidationProblem(errors);

        if (!Enum.TryParse<ApplicationStatus>(request.Status, ignoreCase: true, out var status))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["status"] = [$"'{request.Status}' is not a valid status. Use Submitted, InReview, Interview, Rejected, or Accepted."],
            });
        }

        var company = await CurrentUserResolver.LoadCurrentCompanyAsync(user, db);
        if (company is null)
            return Results.NotFound(new { message = "Company profile not found." });

        var application = await LoadApplication(db, id);
        if (application is null)
            return Results.NotFound();

        if (application.Job.CompanyId != company.Id)
            return Results.Forbid();

        application.Status = status;
        await db.SaveChangesAsync();
        await publisher.PublishAsync(new ApplicationStatusChanged { EntityId = application.Id });

        return Results.Ok(ToResponse(application));
    }

    private static Task<Application?> LoadApplication(CareerProjectDbContext db, Guid id) =>
        db.Applications
            .Include(a => a.Job)
            .Include(a => a.Person)
            .FirstOrDefaultAsync(a => a.Id == id);

    private static ApplicationResponse ToResponse(Application application) => new()
    {
        Id = application.Id,
        JobId = application.JobId,
        JobTitle = application.Job.Title,
        PersonId = application.PersonId,
        PersonName = application.Person.FullName,
        Status = application.Status.ToString(),
        AppliedAt = application.AppliedAt,
    };
}
