using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Features.Discovery;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Api.Controllers;

public class DiscoverController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetFeed()
    {
        var result = await Mediator.Send(new GetHomeFeedQuery());
        return HandleResult(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> Search(
        [FromQuery] string? keyword,
        [FromQuery] Guid? categoryId,
        [FromQuery] ProficiencyLevel? level,
        [FromQuery] double? minRating,
        [FromQuery] string? location,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await Mediator.Send(new SearchSkillsQuery(keyword, categoryId, level, minRating, location, pageNumber, pageSize));
        return HandleResult(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult> GetRecommendedMatches()
    {
        var result = await Mediator.Send(new GetRecommendedMatchesQuery());
        return HandleResult(result);
    }
}
