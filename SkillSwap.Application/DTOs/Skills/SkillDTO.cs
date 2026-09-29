using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class SkillDTO
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
