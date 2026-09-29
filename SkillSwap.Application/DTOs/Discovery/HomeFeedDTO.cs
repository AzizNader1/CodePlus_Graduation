using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class HomeFeedDTO
{
    public ICollection<CategoryDTO> FeaturedCategories { get; set; } = new List<CategoryDTO>();
    public ICollection<PublicUserProfileDTO> TopRatedSwappers { get; set; } = new List<PublicUserProfileDTO>();
    public ICollection<UserSkillDTO> TrendingSkills { get; set; } = new List<UserSkillDTO>();
    public ICollection<SwapMatchDTO> RecommendedForYou { get; set; } = new List<SwapMatchDTO>();
}
