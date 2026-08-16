using System.ComponentModel.DataAnnotations;

namespace CareerProject.UserService.Dtos;

public class UpdateProfileSkillsRequest
{
    [Required]
    public List<ProfileSkillInput> Skills { get; set; } = [];
}

public class ProfileSkillInput
{
    [Required]
    public Guid SkillId { get; set; }

    [Required]
    public string Level { get; set; } = null!;
}
