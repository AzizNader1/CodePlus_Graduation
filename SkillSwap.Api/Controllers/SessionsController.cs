using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Application.Features.Sessions;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Api.Controllers;

[Authorize]
public class SessionsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetSessions([FromQuery] SessionStatus? status)
    {
        var result = await Mediator.Send(new GetSessionsQuery(status));
        return HandleResult(result);
    }

    [HttpGet("{sessionId:guid}")]
    public async Task<ActionResult> GetById(Guid sessionId)
    {
        var result = await Mediator.Send(new GetSessionByIdQuery(sessionId));
        return HandleResult(result);
    }

    [HttpPost("{sessionId:guid}")]
    public async Task<ActionResult> Complete(Guid sessionId, [FromBody] CompleteSessionRequest request)
    {
        var result = await Mediator.Send(new ConfirmSessionCompletionCommand(sessionId, request));
        return HandleResult(result);
    }

    [HttpPost("{sessionId:guid}")]
    public async Task<ActionResult> Cancel(Guid sessionId, [FromBody] CancelSessionRequest request)
    {
        var result = await Mediator.Send(new CancelSessionCommand(sessionId, request));
        return HandleResult(result);
    }

    [HttpPost("{sessionId:guid}")]
    public async Task<ActionResult> Join(Guid sessionId)
    {
        var result = await Mediator.Send(new JoinSessionCallCommand(sessionId));
        return HandleResult(result);
    }

    [HttpPost("{sessionId:guid}")]
    public async Task<ActionResult> Leave(Guid sessionId)
    {
        var result = await Mediator.Send(new LeaveSessionCallCommand(sessionId));
        return HandleResult(result);
    }

    [HttpPost("{sessionId:guid}")]
    public async Task<ActionResult> Heartbeat(Guid sessionId, [FromBody] SessionHeartbeatRequest? request = null)
    {
        var result = await Mediator.Send(new SendSessionHeartbeatCommand(sessionId, request));
        return HandleResult(result);
    }

    [HttpGet("{sessionId:guid}")]
    public async Task<ActionResult> CallStatus(Guid sessionId)
    {
        var result = await Mediator.Send(new GetSessionCallStatusQuery(sessionId));
        return HandleResult(result);
    }

    [HttpGet("{sessionId:guid}")]
    public async Task<ActionResult> IceServers(Guid sessionId)
    {
        var result = await Mediator.Send(new GetIceServersQuery(sessionId));
        return HandleResult(result);
    }
}
