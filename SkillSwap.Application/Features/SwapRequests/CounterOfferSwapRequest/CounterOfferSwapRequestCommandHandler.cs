using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.SwapRequests;

public class CounterOfferSwapRequestCommandHandler : IRequestHandler<CounterOfferSwapRequestCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CounterOfferSwapRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(CounterOfferSwapRequestCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<bool>.Failure("Unauthorized");

        var swapRequest = await _context.SwapRequests.FirstOrDefaultAsync(sr => sr.Id == command.RequestId && !sr.IsDeleted, cancellationToken);
        if (swapRequest == null)
            return Result<bool>.Failure("Swap request not found.");

        if (swapRequest.ReceiverId != _currentUser.UserId.Value)
            return Result<bool>.Failure("Forbidden");

        swapRequest.Status = SwapRequestStatus.CounterOffered;
        swapRequest.CounterProposedDate = command.Request.NewProposedDate;
        swapRequest.CounterDurationMinutes = command.Request.NewDurationMinutes;
        swapRequest.CounterNotes = command.Request.CounterNotes;

        var notif = new Notification
        {
            UserId = swapRequest.RequesterId,
            Title = "Counter-Offer Proposed",
            Message = "A new schedule has been proposed for your swap request.",
            Type = NotificationType.SwapRequestCountered,
            TargetReferenceId = swapRequest.Id
        };
        _context.Notifications.Add(notif);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
