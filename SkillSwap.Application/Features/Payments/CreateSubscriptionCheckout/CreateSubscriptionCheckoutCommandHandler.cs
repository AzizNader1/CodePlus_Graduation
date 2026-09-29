using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Payments;

public class CreateSubscriptionCheckoutCommandHandler : IRequestHandler<CreateSubscriptionCheckoutCommand, Result<StripeCheckoutSessionResult>>
{
    private readonly IStripeService _stripeService;
    private readonly ICurrentUserService _currentUser;

    public CreateSubscriptionCheckoutCommandHandler(IStripeService stripeService, ICurrentUserService currentUser)
    {
        _stripeService = stripeService;
        _currentUser = currentUser;
    }

    public async Task<Result<StripeCheckoutSessionResult>> Handle(CreateSubscriptionCheckoutCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<StripeCheckoutSessionResult>.Failure("Unauthorized");

        var priceId = command.Request.Plan == SubscriptionPlan.ProYearly ? "price_pro_yearly" : "price_pro_monthly";

        var sessionResult = await _stripeService.CreateSubscriptionCheckoutSessionAsync(
            _currentUser.UserId.Value,
            priceId,
            command.Request.SuccessUrl,
            command.Request.CancelUrl);

        return Result<StripeCheckoutSessionResult>.Success(sessionResult);
    }
}
