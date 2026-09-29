using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;

namespace SkillSwap.Application.Features.Sessions;

public class GetSessionCallStatusQueryHandler : IRequestHandler<GetSessionCallStatusQuery, Result<SessionCallStatusDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSessionCallStatusQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<SessionCallStatusDTO>> Handle(GetSessionCallStatusQuery query, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<SessionCallStatusDTO>.Failure("Unauthorized");

        var session = await _context.SwapSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.SessionId && !s.IsDeleted, cancellationToken);

        if (session == null)
            return Result<SessionCallStatusDTO>.Failure("Session not found.");

        var elapsedMinutes = 0;
        if (session.ActualStartTime.HasValue)
        {
            var endTime = session.ActualEndTime ?? DateTime.UtcNow;
            elapsedMinutes = (int)Math.Max(0, (endTime - session.ActualStartTime.Value).TotalMinutes);
        }

        return Result<SessionCallStatusDTO>.Success(new SessionCallStatusDTO
        {
            SessionId = session.Id,
            IsHostInCall = session.IsHostInCall,
            IsParticipantInCall = session.IsParticipantInCall,
            HostJoinedAt = session.HostJoinedAt,
            ParticipantJoinedAt = session.ParticipantJoinedAt,
            HostLastHeartbeatAt = session.HostLastHeartbeatAt,
            ParticipantLastHeartbeatAt = session.ParticipantLastHeartbeatAt,
            ElapsedCallMinutes = elapsedMinutes,
            ActualDurationMinutes = session.ActualDurationMinutes,
            Status = session.Status
        });
    }
}
