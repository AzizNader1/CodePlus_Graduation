using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Application.Features.Skills;

namespace SkillSwap.Api.Controllers;

public class SkillsController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetSkills([FromQuery] Guid? categoryId, [FromQuery] string? search)
    {
        var result = await Mediator.Send(new GetSkillsQuery(categoryId, search));
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> AddOfferedSkill([FromBody] AddOfferedSkillRequest request)
    {
        var result = await Mediator.Send(new AddOfferedSkillCommand(request));
        return HandleResult(result);
    }

    [HttpDelete("{userSkillId:guid}")]
    [Authorize]
    public async Task<ActionResult> DeleteOfferedSkill(Guid userSkillId)
    {
        var result = await Mediator.Send(new DeleteOfferedSkillCommand(userSkillId));
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> AddWantedSkill([FromBody] AddWantedSkillRequest request)
    {
        var result = await Mediator.Send(new AddWantedSkillCommand(request));
        return HandleResult(result);
    }

    [HttpDelete("{wantedSkillId:guid}")]
    [Authorize]
    public async Task<ActionResult> DeleteWantedSkill(Guid wantedSkillId)
    {
        var result = await Mediator.Send(new DeleteWantedSkillCommand(wantedSkillId));
        return HandleResult(result);
    }
}
