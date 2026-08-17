using CareerProject.JobService.Services;
using Xunit;

namespace CareerProject.JobService.Tests.Unit;

public class ApplicationRulesTests
{
    [Fact]
    public void IsDuplicate_JobNotInExistingApplications_ReturnsFalse()
    {
        var jobId = Guid.NewGuid();
        var existing = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        Assert.False(ApplicationRules.IsDuplicate(existing, jobId));
    }

    [Fact]
    public void IsDuplicate_JobAlreadyInExistingApplications_ReturnsTrue()
    {
        var jobId = Guid.NewGuid();
        var existing = new List<Guid> { Guid.NewGuid(), jobId };

        Assert.True(ApplicationRules.IsDuplicate(existing, jobId));
    }

    [Fact]
    public void IsDuplicate_NoExistingApplications_ReturnsFalse()
    {
        Assert.False(ApplicationRules.IsDuplicate([], Guid.NewGuid()));
    }
}
