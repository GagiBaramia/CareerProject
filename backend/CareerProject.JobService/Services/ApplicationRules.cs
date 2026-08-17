namespace CareerProject.JobService.Services;

// Pure business rule extracted from ApplicationEndpoints - the DB also enforces this via a
// unique index on (JobId, PersonId), but this is the app-level check that produces a clean
// 409 instead of the caller hitting a raw DB constraint violation.
public static class ApplicationRules
{
    public static bool IsDuplicate(IEnumerable<Guid> jobIdsAlreadyAppliedTo, Guid jobId) =>
        jobIdsAlreadyAppliedTo.Contains(jobId);
}
