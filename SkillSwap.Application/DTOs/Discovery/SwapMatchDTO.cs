using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class SwapMatchDTO
{
    public PublicUserProfileDTO PartnerUser { get; set; } = null!;
    public string YouTeachSkill { get; set; } = string.Empty;
    public string TheyTeachSkill { get; set; } = string.Empty;
    public int MatchCompatibilityScore { get; set; } // 0 - 100%
}
