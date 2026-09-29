using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Application.Features.Admin;

namespace SkillSwap.Api.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetStats()
    {
        var result = await Mediator.Send(new GetAdminStatsQuery());
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetReports()
    {
        var result = await Mediator.Send(new GetAdminReportsQuery());
        return HandleResult(result);
    }

    [HttpPut("{reportId:guid}")]
    public async Task<ActionResult> ResolveReport(Guid reportId, [FromBody] ResolveReportRequest request)
    {
        var result = await Mediator.Send(new ResolveReportCommand(reportId, request));
        return HandleResult(result);
    }

    [HttpPut("{userId:guid}")]
    public async Task<ActionResult> UpdateUserStatus(Guid userId, [FromQuery] bool isActive)
    {
        var result = await Mediator.Send(new UpdateUserStatusCommand(userId, isActive));
        return HandleResult(result);
    }
}
