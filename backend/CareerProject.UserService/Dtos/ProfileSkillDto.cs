namespace CareerProject.UserService.Dtos;

public class ProfileSkillDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = null!;
    public string Level { get; set; } = null!;
}
