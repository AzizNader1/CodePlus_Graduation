using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Application.Features.SwapRequests;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Api.Controllers;

[Authorize]
public class SwapRequestsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateSwapRequest request)
    {
        var result = await Mediator.Send(new CreateSwapRequestCommand(request));
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetIncoming([FromQuery] SwapRequestStatus? status)
    {
        var result = await Mediator.Send(new GetIncomingSwapRequestsQuery(status));
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetOutgoing([FromQuery] SwapRequestStatus? status)
    {
        var result = await Mediator.Send(new GetOutgoingSwapRequestsQuery(status));
        return HandleResult(result);
    }

    [HttpPost("{swapRequestId:guid}")]
    public async Task<ActionResult> Accept(Guid swapRequestId)
    {
        var result = await Mediator.Send(new AcceptSwapRequestCommand(swapRequestId));
        return HandleResult(result);
    }

    [HttpPost("{swapRequestId:guid}")]
    public async Task<ActionResult> Reject(Guid swapRequestId, [FromBody] RejectRequest request)
    {
        var result = await Mediator.Send(new RejectSwapRequestCommand(swapRequestId, request));
        return HandleResult(result);
    }

    [HttpPost("{swapRequestId:guid}")]
    public async Task<ActionResult> CounterOffer(Guid swapRequestId, [FromBody] CounterOfferRequest request)
    {
        var result = await Mediator.Send(new CounterOfferSwapRequestCommand(swapRequestId, request));
        return HandleResult(result);
    }
}
