using System.ComponentModel.DataAnnotations;
using CareerProject.Shared.Validation;
using Xunit;

namespace CareerProject.Shared.Tests.Unit;

public class RequestValidatorTests
{
    private class TestRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, MinLength(8)]
        public string Password { get; set; } = null!;
    }

    [Fact]
    public void TryValidate_AllFieldsValid_ReturnsTrue()
    {
        var request = new TestRequest { Email = "person@example.com", Password = "longenough" };

        var isValid = RequestValidator.TryValidate(request, out var errors);

        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void TryValidate_MissingRequiredField_ReturnsFalseWithFieldError()
    {
        var request = new TestRequest { Email = "person@example.com", Password = "" };

        var isValid = RequestValidator.TryValidate(request, out var errors);

        Assert.False(isValid);
        Assert.Contains(nameof(TestRequest.Password), errors.Keys);
    }

    [Fact]
    public void TryValidate_InvalidEmailFormat_ReturnsFalseWithFieldError()
    {
        var request = new TestRequest { Email = "not-an-email", Password = "longenough" };

        var isValid = RequestValidator.TryValidate(request, out var errors);

        Assert.False(isValid);
        Assert.Contains(nameof(TestRequest.Email), errors.Keys);
    }

    [Fact]
    public void TryValidate_PasswordTooShort_ReturnsFalseWithFieldError()
    {
        var request = new TestRequest { Email = "person@example.com", Password = "short" };

        var isValid = RequestValidator.TryValidate(request, out var errors);

        Assert.False(isValid);
        Assert.Contains(nameof(TestRequest.Password), errors.Keys);
    }

    [Fact]
    public void TryValidate_MultipleInvalidFields_ReturnsAllFieldErrors()
    {
        var request = new TestRequest { Email = "not-an-email", Password = "short" };

        var isValid = RequestValidator.TryValidate(request, out var errors);

        Assert.False(isValid);
        Assert.Equal(2, errors.Count);
        Assert.Contains(nameof(TestRequest.Email), errors.Keys);
        Assert.Contains(nameof(TestRequest.Password), errors.Keys);
    }
}
