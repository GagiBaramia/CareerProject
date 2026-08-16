namespace CareerProject.Shared.Entities;

public class PersonSkill
{
    public Guid PersonId { get; set; }
    public Guid SkillId { get; set; }
    public int Level { get; set; }

    public PersonProfile Person { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
