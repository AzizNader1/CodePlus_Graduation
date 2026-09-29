using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.SwapRequests;

public class RejectSwapRequestCommandHandler : IRequestHandler<RejectSwapRequestCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RejectSwapRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(RejectSwapRequestCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<bool>.Failure("Unauthorized");

        var swapRequest = await _context.SwapRequests.FirstOrDefaultAsync(sr => sr.Id == command.RequestId && !sr.IsDeleted, cancellationToken);
        if (swapRequest == null)
            return Result<bool>.Failure("Swap request not found.");

        if (swapRequest.ReceiverId != _currentUser.UserId.Value)
            return Result<bool>.Failure("Forbidden");

        swapRequest.Status = SwapRequestStatus.Rejected;
        swapRequest.RejectionReason = command.Request.Reason;

        var notif = new Notification
        {
            UserId = swapRequest.RequesterId,
            Title = "Swap Request Declined",
            Message = "Your swap proposal was declined.",
            Type = NotificationType.SwapRequestRejected,
            TargetReferenceId = swapRequest.Id
        };
        _context.Notifications.Add(notif);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
