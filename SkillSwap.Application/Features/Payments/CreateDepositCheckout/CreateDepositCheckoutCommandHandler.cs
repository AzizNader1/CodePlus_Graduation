using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Payments;

public class CreateDepositCheckoutCommandHandler : IRequestHandler<CreateDepositCheckoutCommand, Result<StripeCheckoutSessionResult>>
{
    private readonly IStripeService _stripeService;
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationDbContext _context;

    public CreateDepositCheckoutCommandHandler(
        IStripeService stripeService,
        ICurrentUserService currentUser,
        IApplicationDbContext context)
    {
        _stripeService = stripeService;
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<Result<StripeCheckoutSessionResult>> Handle(CreateDepositCheckoutCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<StripeCheckoutSessionResult>.Failure("Unauthorized");

        var sessionResult = await _stripeService.CreateDepositCheckoutSessionAsync(
            _currentUser.UserId.Value,
            command.Request.Amount,
            command.Request.SuccessUrl,
            command.Request.CancelUrl);

        var transaction = new PaymentTransaction
        {
            UserId = _currentUser.UserId.Value,
            StripeSessionId = sessionResult.SessionId,
            Amount = command.Request.Amount,
            Currency = "usd",
            Status = PaymentStatus.Pending,
            Description = "Skill Swap Session Escrow Deposit"
        };
        _context.PaymentTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<StripeCheckoutSessionResult>.Success(sessionResult);
    }
}
