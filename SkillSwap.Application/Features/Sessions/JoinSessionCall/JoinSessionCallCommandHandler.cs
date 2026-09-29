using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Sessions;

public class JoinSessionCallCommandHandler : IRequestHandler<JoinSessionCallCommand, Result<JoinSessionCallResponseDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IVideoCallService _videoCallService;

    public JoinSessionCallCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IVideoCallService videoCallService)
    {
        _context = context;
        _currentUser = currentUser;
        _videoCallService = videoCallService;
    }

    public async Task<Result<JoinSessionCallResponseDTO>> Handle(JoinSessionCallCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<JoinSessionCallResponseDTO>.Failure("Unauthorized");

        var userId = _currentUser.UserId.Value;

        var session = await _context.SwapSessions
            .Include(s => s.HostUser)
            .Include(s => s.ParticipantUser)
            .FirstOrDefaultAsync(s => s.Id == command.SessionId && !s.IsDeleted, cancellationToken);

        if (session == null)
            return Result<JoinSessionCallResponseDTO>.Failure("Session not found.");

        if (session.HostUserId != userId && session.ParticipantUserId != userId)
            return Result<JoinSessionCallResponseDTO>.Failure("You are not an authorized participant in this session.");

        if (session.Status == SessionStatus.Cancelled)
            return Result<JoinSessionCallResponseDTO>.Failure("Cannot join a cancelled session.");

        var isHost = session.HostUserId == userId;
        var role = isHost ? "Host" : "Participant";
        var partnerUser = isHost ? session.ParticipantUser : session.HostUser;

        // Transition status if scheduled
        if (session.Status == SessionStatus.Scheduled)
        {
            session.Status = SessionStatus.InProgress;
        }

        session.ActualStartTime ??= DateTime.UtcNow;

        if (isHost)
        {
            session.HostJoinedAt ??= DateTime.UtcNow;
            session.IsHostInCall = true;
            session.HostLastHeartbeatAt = DateTime.UtcNow;
        }
        else
        {
            session.ParticipantJoinedAt ??= DateTime.UtcNow;
            session.IsParticipantInCall = true;
            session.ParticipantLastHeartbeatAt = DateTime.UtcNow;
        }

        var roomToken = _videoCallService.GenerateRoomToken(session.Id, userId, role);
        session.RoomSecurityToken = roomToken;

        await _context.SaveChangesAsync(cancellationToken);

        var iceServers = _videoCallService.GetIceServers();

        return Result<JoinSessionCallResponseDTO>.Success(new JoinSessionCallResponseDTO
        {
            SessionId = session.Id,
            RoomId = $"room_{session.Id:N}",
            RoomSecurityToken = roomToken,
            ParticipantRole = role,
            PartnerName = partnerUser?.FullName ?? "Partner",
            PartnerAvatarUrl = partnerUser?.AvatarUrl,
            PartnerUserId = partnerUser?.Id ?? Guid.Empty,
            ScheduledStartTime = session.ScheduledStartTime,
            ScheduledEndTime = session.ScheduledEndTime,
            DurationMinutes = session.DurationMinutes,
            Status = session.Status,
            IceServers = iceServers.ToList()
        });
    }
}
