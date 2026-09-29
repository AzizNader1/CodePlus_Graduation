using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class CategoryDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public int DisplayOrder { get; set; }
    public int SkillsCount { get; set; }
}
