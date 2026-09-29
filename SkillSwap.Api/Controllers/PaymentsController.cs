using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Application.Features.Payments;

namespace SkillSwap.Api.Controllers;

[Authorize]
public class PaymentsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateDepositCheckout([FromBody] CreateDepositCheckoutRequest request)
    {
        var result = await Mediator.Send(new CreateDepositCheckoutCommand(request));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateSubscriptionCheckout([FromBody] CreateSubscriptionCheckoutRequest request)
    {
        var result = await Mediator.Send(new CreateSubscriptionCheckoutCommand(request));
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetPaymentHistory()
    {
        var result = await Mediator.Send(new GetMyPaymentTransactionsQuery());
        return HandleResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        // Webhook received and acknowledged
        return Ok(new { received = true });
    }
}
