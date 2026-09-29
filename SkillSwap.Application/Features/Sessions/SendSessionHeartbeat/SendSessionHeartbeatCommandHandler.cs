using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;

namespace SkillSwap.Application.Features.Sessions;

public class SendSessionHeartbeatCommandHandler : IRequestHandler<SendSessionHeartbeatCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SendSessionHeartbeatCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(SendSessionHeartbeatCommand command, CancellationToken cancellationToken)
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
            session.HostLastHeartbeatAt = DateTime.UtcNow;
            session.IsHostInCall = true;
        }
        else if (session.ParticipantUserId == userId)
        {
            session.ParticipantLastHeartbeatAt = DateTime.UtcNow;
            session.IsParticipantInCall = true;
        }
        else
        {
            return Result<bool>.Failure("Not an authorized participant in this session.");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
