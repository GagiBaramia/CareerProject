namespace CareerProject.Shared.Entities;

public class JobSkill
{
    public Guid JobId { get; set; }
    public Guid SkillId { get; set; }
    public int RequiredLevel { get; set; }

    public Job Job { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
