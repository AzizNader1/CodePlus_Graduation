using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;

namespace SkillSwap.Application.Features.Sessions;

public class LeaveSessionCallCommandHandler : IRequestHandler<LeaveSessionCallCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public LeaveSessionCallCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(LeaveSessionCallCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<bool>.Failure("Unauthorized");

        var userId = _currentUser.UserId.Value;

        var session = await _context.SwapSessions
            .FirstOrDefaultAsync(s => s.Id == command.SessionId && !s.IsDeleted, cancellationToken);

        if (session == null)
            return Result<bool>.Failure("Session not found.");

        if (session.HostUserId == userId)
        {
            session.IsHostInCall = false;
        }
        else if (session.ParticipantUserId == userId)
        {
            session.IsParticipantInCall = false;
        }
        else
        {
            return Result<bool>.Failure("Not an authorized participant in this session.");
        }

        if (!session.IsHostInCall && !session.IsParticipantInCall && session.ActualStartTime.HasValue)
        {
            session.ActualEndTime = DateTime.UtcNow;
            session.ActualDurationMinutes = (int)Math.Max(1, (session.ActualEndTime.Value - session.ActualStartTime.Value).TotalMinutes);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
