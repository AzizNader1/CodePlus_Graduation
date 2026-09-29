using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Application.Features.Chat;

namespace SkillSwap.Api.Controllers;

[Authorize]
public class ChatController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetConversations()
    {
        var result = await Mediator.Send(new GetConversationsQuery());
        return HandleResult(result);
    }

    [HttpGet("{conversationId:guid}")]
    public async Task<ActionResult> GetMessages(Guid conversationId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 30)
    {
        var result = await Mediator.Send(new GetMessagesQuery(conversationId, pageNumber, pageSize));
        return HandleResult(result);
    }

    [HttpPost("{conversationId:guid}")]
    public async Task<ActionResult> SendMessage(Guid conversationId, [FromBody] SendMessageRequest request)
    {
        var result = await Mediator.Send(new SendMessageCommand(conversationId, request));
        return HandleResult(result);
    }
}
