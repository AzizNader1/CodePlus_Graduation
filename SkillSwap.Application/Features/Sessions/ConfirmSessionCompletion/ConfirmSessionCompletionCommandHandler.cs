using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Sessions;

public class ConfirmSessionCompletionCommandHandler : IRequestHandler<ConfirmSessionCompletionCommand, Result<SwapSessionDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISignalRNotificationService _signalR;

    public ConfirmSessionCompletionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ISignalRNotificationService signalR)
    {
        _context = context;
        _currentUser = currentUser;
        _signalR = signalR;
    }

    public async Task<Result<SwapSessionDTO>> Handle(ConfirmSessionCompletionCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<SwapSessionDTO>.Failure("Unauthorized");

        var userId = _currentUser.UserId.Value;
        var session = await _context.SwapSessions
            .Include(s => s.HostUser)
            .Include(s => s.ParticipantUser)
            .FirstOrDefaultAsync(s => s.Id == command.Id && !s.IsDeleted, cancellationToken);

        if (session == null)
            return Result<SwapSessionDTO>.Failure("Session not found.");

        if (session.HostUserId != userId && session.ParticipantUserId != userId)
            return Result<SwapSessionDTO>.Failure("Forbidden");

        if (session.Status == SessionStatus.Cancelled)
            return Result<SwapSessionDTO>.Failure("Cannot complete a cancelled session.");

        if (session.HostUserId == userId)
        {
            session.HostConfirmedCompleted = true;
            if (!string.IsNullOrEmpty(command.Request.Notes))
                session.HostNotes = command.Request.Notes;
        }
        else
        {
            session.ParticipantConfirmedCompleted = true;
            if (!string.IsNullOrEmpty(command.Request.Notes))
                session.ParticipantNotes = command.Request.Notes;
        }

        if (session.HostConfirmedCompleted && session.ParticipantConfirmedCompleted)
        {
            session.Status = SessionStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;

            session.HostUser.TotalSwapsCompleted++;
            session.ParticipantUser.TotalSwapsCompleted++;

            var otherUserId = session.HostUserId == userId ? session.ParticipantUserId : session.HostUserId;
            var notif = new Notification
            {
                UserId = otherUserId,
                Title = "Session Completed! Leave a Review",
                Message = "Your swap session has concluded. Please rate your experience to build platform trust.",
                Type = NotificationType.SessionCompleted,
                TargetReferenceId = session.Id
            };
            _context.Notifications.Add(notif);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<SwapSessionDTO>.Success(new SwapSessionDTO
        {
            Id = session.Id,
            SwapRequestId = session.SwapRequestId,
            HostUserId = session.HostUserId,
            HostName = session.HostUser.FullName,
            HostAvatarUrl = session.HostUser.AvatarUrl,
            ParticipantUserId = session.ParticipantUserId,
            ParticipantName = session.ParticipantUser.FullName,
            ParticipantAvatarUrl = session.ParticipantUser.AvatarUrl,
            ScheduledStartTime = session.ScheduledStartTime,
            ScheduledEndTime = session.ScheduledEndTime,
            DurationMinutes = session.DurationMinutes,
            MeetingLink = session.MeetingLink,
            Status = session.Status,
            HostConfirmedCompleted = session.HostConfirmedCompleted,
            ParticipantConfirmedCompleted = session.ParticipantConfirmedCompleted,
            CompletedAt = session.CompletedAt
        });
    }
}
