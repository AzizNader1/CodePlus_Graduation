using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Features.Notifications;

namespace SkillSwap.Api.Controllers;

[Authorize]
public class NotificationsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetNotifications([FromQuery] bool? unreadOnly, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetNotificationsQuery(unreadOnly, pageNumber, pageSize));
        return HandleResult(result);
    }

    [HttpPut("{notificationId:guid}")]
    public async Task<ActionResult> MarkAsRead(Guid notificationId)
    {
        var result = await Mediator.Send(new MarkNotificationAsReadCommand(notificationId));
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var result = await Mediator.Send(new MarkAllNotificationsAsReadCommand());
        return HandleResult(result);
    }
}
