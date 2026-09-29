using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Sessions;

public class CancelSessionCommandHandler : IRequestHandler<CancelSessionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CancelSessionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(CancelSessionCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<bool>.Failure("Unauthorized");

        var session = await _context.SwapSessions.FirstOrDefaultAsync(s => s.Id == command.Id && !s.IsDeleted, cancellationToken);
        if (session == null)
            return Result<bool>.Failure("Session not found.");

        if (session.HostUserId != _currentUser.UserId.Value && session.ParticipantUserId != _currentUser.UserId.Value)
            return Result<bool>.Failure("Forbidden");

        session.Status = SessionStatus.Cancelled;
        session.CancellationReason = command.Request.Reason;
        session.CancelledByUserId = _currentUser.UserId.Value;

        var otherUserId = session.HostUserId == _currentUser.UserId.Value ? session.ParticipantUserId : session.HostUserId;
        _context.Notifications.Add(new Notification
        {
            UserId = otherUserId,
            Title = "Session Cancelled",
            Message = $"Your scheduled session was cancelled: {command.Request.Reason}",
            Type = NotificationType.SystemAlert,
            TargetReferenceId = session.Id
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
