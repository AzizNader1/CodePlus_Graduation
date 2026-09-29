using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.SwapRequests;

public class AcceptSwapRequestCommandHandler : IRequestHandler<AcceptSwapRequestCommand, Result<SwapSessionDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISignalRNotificationService _signalR;

    public AcceptSwapRequestCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ISignalRNotificationService signalR)
    {
        _context = context;
        _currentUser = currentUser;
        _signalR = signalR;
    }

    public async Task<Result<SwapSessionDTO>> Handle(AcceptSwapRequestCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<SwapSessionDTO>.Failure("Unauthorized");

        var swapRequest = await _context.SwapRequests
            .Include(sr => sr.Requester)
            .Include(sr => sr.Receiver)
            .FirstOrDefaultAsync(sr => sr.Id == command.RequestId && !sr.IsDeleted, cancellationToken);

        if (swapRequest == null)
            return Result<SwapSessionDTO>.Failure("Swap request not found.");

        if (swapRequest.ReceiverId != _currentUser.UserId.Value)
            return Result<SwapSessionDTO>.Failure("Only the recipient of this swap request can accept it.");

        if (swapRequest.Status != SwapRequestStatus.Pending && swapRequest.Status != SwapRequestStatus.CounterOffered)
            return Result<SwapSessionDTO>.Failure($"Request is in {swapRequest.Status} state and cannot be accepted.");

        swapRequest.Status = SwapRequestStatus.Accepted;

        var sessionStartTime = swapRequest.CounterProposedDate ?? swapRequest.ProposedDate;
        var duration = swapRequest.CounterDurationMinutes ?? swapRequest.DurationMinutes;

        var session = new SwapSession
        {
            SwapRequestId = swapRequest.Id,
            HostUserId = swapRequest.ReceiverId,
            ParticipantUserId = swapRequest.RequesterId,
            ScheduledStartTime = sessionStartTime,
            ScheduledEndTime = sessionStartTime.AddMinutes(duration),
            DurationMinutes = duration,
            MeetingLink = $"https://meet.skillswap.app/room/{Guid.NewGuid():N}",
            Status = SessionStatus.Scheduled
        };

        _context.SwapSessions.Add(session);

        var conv = await _context.Conversations.FirstOrDefaultAsync(c =>
            (c.UserOneId == swapRequest.RequesterId && c.UserTwoId == swapRequest.ReceiverId) ||
            (c.UserOneId == swapRequest.ReceiverId && c.UserTwoId == swapRequest.RequesterId), cancellationToken);

        if (conv == null)
        {
            conv = new Conversation
            {
                UserOneId = swapRequest.ReceiverId,
                UserTwoId = swapRequest.RequesterId,
                SwapRequestId = swapRequest.Id
            };
            _context.Conversations.Add(conv);
        }

        var welcomeMsg = new Message
        {
            Conversation = conv,
            SenderId = swapRequest.ReceiverId,
            Content = $"Swap proposal accepted! Session scheduled for {session.ScheduledStartTime:g}. Meeting Room: {session.MeetingLink}",
            SentAt = DateTime.UtcNow
        };
        _context.Messages.Add(welcomeMsg);

        var notif = new Notification
        {
            UserId = swapRequest.RequesterId,
            Title = "Swap Request Accepted!",
            Message = $"{swapRequest.Receiver.FullName} accepted your swap request. Session is scheduled for {session.ScheduledStartTime:g}.",
            Type = NotificationType.SwapRequestAccepted,
            TargetReferenceId = session.Id
        };
        _context.Notifications.Add(notif);

        await _context.SaveChangesAsync(cancellationToken);

        await _signalR.SendMessageToUserAsync(swapRequest.RequesterId, "ReceiveNotification", new
        {
            notif.Id,
            notif.Title,
            notif.Message,
            notif.Type
        });

        return Result<SwapSessionDTO>.Success(new SwapSessionDTO
        {
            Id = session.Id,
            SwapRequestId = session.SwapRequestId,
            HostUserId = session.HostUserId,
            HostName = swapRequest.Receiver.FullName,
            HostAvatarUrl = swapRequest.Receiver.AvatarUrl,
            ParticipantUserId = session.ParticipantUserId,
            ParticipantName = swapRequest.Requester.FullName,
            ParticipantAvatarUrl = swapRequest.Requester.AvatarUrl,
            ScheduledStartTime = session.ScheduledStartTime,
            ScheduledEndTime = session.ScheduledEndTime,
            DurationMinutes = session.DurationMinutes,
            MeetingLink = session.MeetingLink,
            Status = session.Status
        });
    }
}
