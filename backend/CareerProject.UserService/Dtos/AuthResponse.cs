namespace CareerProject.UserService.Dtos;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
}
