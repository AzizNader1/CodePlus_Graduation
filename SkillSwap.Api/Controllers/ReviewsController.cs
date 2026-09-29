using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Application.Features.Reviews;

namespace SkillSwap.Api.Controllers;

public class ReviewsController : ApiControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var result = await Mediator.Send(new CreateReviewCommand(request));
        return HandleResult(result);
    }

    [HttpGet("{userId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetUserReviews(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await Mediator.Send(new GetUserReviewsQuery(userId, pageNumber, pageSize));
        return HandleResult(result);
    }
}
