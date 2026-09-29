using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.SwapRequests;

public class CreateSwapRequestCommandHandler : IRequestHandler<CreateSwapRequestCommand, Result<SwapRequestDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISignalRNotificationService _signalR;

    public CreateSwapRequestCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ISignalRNotificationService signalR)
    {
        _context = context;
        _currentUser = currentUser;
        _signalR = signalR;
    }

    public async Task<Result<SwapRequestDTO>> Handle(CreateSwapRequestCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<SwapRequestDTO>.Failure("Unauthorized");

        var req = command.Request;
        if (req.ReceiverId == _currentUser.UserId.Value)
            return Result<SwapRequestDTO>.Failure("You cannot initiate a skill swap with yourself.");

        var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == req.ReceiverId && u.IsActive, cancellationToken);
        if (receiver == null)
            return Result<SwapRequestDTO>.Failure("Target receiver does not exist or is inactive.");

        var offeredSkill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == req.OfferedSkillId, cancellationToken);
        var requestedSkill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == req.RequestedSkillId, cancellationToken);
        if (offeredSkill == null || requestedSkill == null)
            return Result<SwapRequestDTO>.Failure("Selected skills are invalid.");

        var swapRequest = new SwapRequest
        {
            RequesterId = _currentUser.UserId.Value,
            ReceiverId = req.ReceiverId,
            OfferedSkillId = req.OfferedSkillId,
            RequestedSkillId = req.RequestedSkillId,
            ProposedDate = req.ProposedDate,
            DurationMinutes = req.DurationMinutes > 0 ? req.DurationMinutes : 60,
            Status = SwapRequestStatus.Pending,
            Notes = req.Notes
        };

        _context.SwapRequests.Add(swapRequest);

        var notification = new Notification
        {
            UserId = req.ReceiverId,
            Title = "New Skill Swap Proposal",
            Message = $"You received a swap request to trade {requestedSkill.Name} for {offeredSkill.Name}.",
            Type = NotificationType.SwapRequestReceived,
            TargetReferenceId = swapRequest.Id
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync(cancellationToken);

        await _signalR.SendMessageToUserAsync(req.ReceiverId, "ReceiveNotification", new
        {
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Type
        });

        var requester = await _context.Users.FindAsync(new object[] { _currentUser.UserId.Value }, cancellationToken);

        return Result<SwapRequestDTO>.Success(new SwapRequestDTO
        {
            Id = swapRequest.Id,
            RequesterId = swapRequest.RequesterId,
            RequesterName = requester!.FullName,
            RequesterAvatarUrl = requester.AvatarUrl,
            RequesterRating = requester.AverageRating,
            ReceiverId = receiver.Id,
            ReceiverName = receiver.FullName,
            ReceiverAvatarUrl = receiver.AvatarUrl,
            ReceiverRating = receiver.AverageRating,
            OfferedSkillId = offeredSkill.Id,
            OfferedSkillName = offeredSkill.Name,
            RequestedSkillId = requestedSkill.Id,
            RequestedSkillName = requestedSkill.Name,
            ProposedDate = swapRequest.ProposedDate,
            DurationMinutes = swapRequest.DurationMinutes,
            Status = swapRequest.Status,
            Notes = swapRequest.Notes,
            CreatedAt = swapRequest.CreatedAt
        });
    }
}
