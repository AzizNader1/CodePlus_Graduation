using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Application.Features.Reports;

namespace SkillSwap.Api.Controllers;

public class ReportsController : ApiControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateReport([FromBody] CreateReportRequest request)
    {
        var result = await Mediator.Send(new CreateReportCommand(request));
        return HandleResult(result);
    }
}
