using CareerProject.Shared.Validation;
using CareerProject.UserService.Dtos;
using Xunit;

namespace CareerProject.UserService.Tests.Unit;

// "Role validation" here means the request-shape validation that gates whether a role-specific
// registration (Person vs Company) is even allowed to proceed to creating a User - a malformed
// request must never reach the point of assigning a role.
public class RegistrationRoleValidationTests
{
    [Fact]
    public void RegisterPerson_ValidRequest_PassesValidation()
    {
        var request = new RegisterPersonRequest { Email = "nino@example.com", Password = "longenough", FullName = "Nino" };

        Assert.True(RequestValidator.TryValidate(request, out _));
    }

    [Fact]
    public void RegisterPerson_MissingFullName_FailsValidation()
    {
        var request = new RegisterPersonRequest { Email = "nino@example.com", Password = "longenough", FullName = "" };

        var isValid = RequestValidator.TryValidate(request, out var errors);

        Assert.False(isValid);
        Assert.Contains(nameof(RegisterPersonRequest.FullName), errors.Keys);
    }

    [Fact]
    public void RegisterPerson_ShortPassword_FailsValidation()
    {
        var request = new RegisterPersonRequest { Email = "nino@example.com", Password = "short", FullName = "Nino" };

        var isValid = RequestValidator.TryValidate(request, out var errors);

        Assert.False(isValid);
        Assert.Contains(nameof(RegisterPersonRequest.Password), errors.Keys);
    }

    [Fact]
    public void RegisterCompany_ValidRequest_PassesValidation()
    {
        var request = new RegisterCompanyRequest { Email = "hr@tbc.ge", Password = "longenough", CompanyName = "TBC Bank" };

        Assert.True(RequestValidator.TryValidate(request, out _));
    }

    [Fact]
    public void RegisterCompany_MissingCompanyName_FailsValidation()
    {
        var request = new RegisterCompanyRequest { Email = "hr@tbc.ge", Password = "longenough", CompanyName = "" };

        var isValid = RequestValidator.TryValidate(request, out var errors);

        Assert.False(isValid);
        Assert.Contains(nameof(RegisterCompanyRequest.CompanyName), errors.Keys);
    }

    [Fact]
    public void RegisterCompany_InvalidEmail_FailsValidation()
    {
        var request = new RegisterCompanyRequest { Email = "not-an-email", Password = "longenough", CompanyName = "TBC Bank" };

        var isValid = RequestValidator.TryValidate(request, out var errors);

        Assert.False(isValid);
        Assert.Contains(nameof(RegisterCompanyRequest.Email), errors.Keys);
    }

    [Fact]
    public void Login_MissingPassword_FailsValidation()
    {
        var request = new LoginRequest { Email = "nino@example.com", Password = "" };

        var isValid = RequestValidator.TryValidate(request, out var errors);

        Assert.False(isValid);
        Assert.Contains(nameof(LoginRequest.Password), errors.Keys);
    }
}
