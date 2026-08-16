namespace CareerProject.Shared.Entities;

public class Skill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<PersonSkill> PersonSkills { get; set; } = new List<PersonSkill>();
    public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
}
