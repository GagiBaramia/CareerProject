using System.ComponentModel.DataAnnotations;

namespace CareerProject.UserService.Dtos;

public class RegisterPersonRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required, MinLength(8)]
    public string Password { get; set; } = null!;

    [Required, MinLength(1)]
    public string FullName { get; set; } = null!;
}
