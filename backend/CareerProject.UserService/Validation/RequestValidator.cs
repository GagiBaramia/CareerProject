using System.ComponentModel.DataAnnotations;

namespace CareerProject.UserService.Validation;

public static class RequestValidator
{
    public static bool TryValidate<T>(T request, out IDictionary<string, string[]> errors) where T : notnull
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

        errors = results
            .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty).Select(m => (Member: m, r.ErrorMessage)))
            .GroupBy(x => x.Member)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage ?? "Invalid value.").ToArray());

        return isValid;
    }
}
