using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Chat;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<MessageDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISignalRNotificationService _signalR;

    public SendMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ISignalRNotificationService signalR)
    {
        _context = context;
        _currentUser = currentUser;
        _signalR = signalR;
    }

    public async Task<Result<MessageDTO>> Handle(SendMessageCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<MessageDTO>.Failure("Unauthorized");

        var conversation = await _context.Conversations
            .Include(c => c.UserOne)
            .Include(c => c.UserTwo)
            .FirstOrDefaultAsync(c => c.Id == command.ConversationId && !c.IsDeleted, cancellationToken);

        if (conversation == null)
            return Result<MessageDTO>.Failure("Conversation not found.");

        if (conversation.UserOneId != _currentUser.UserId.Value && conversation.UserTwoId != _currentUser.UserId.Value)
            return Result<MessageDTO>.Failure("Forbidden");

        var sender = conversation.UserOneId == _currentUser.UserId.Value ? conversation.UserOne : conversation.UserTwo;
        var partnerId = conversation.UserOneId == _currentUser.UserId.Value ? conversation.UserTwoId : conversation.UserOneId;

        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = sender.Id,
            Content = command.Request.Content,
            AttachmentUrl = command.Request.AttachmentUrl,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);
        conversation.LastMessageAt = message.SentAt;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new MessageDTO
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = sender.Id,
            SenderName = sender.FullName,
            Content = message.Content,
            AttachmentUrl = message.AttachmentUrl,
            IsRead = message.IsRead,
            SentAt = message.SentAt
        };

        await _signalR.BroadcastToConversationAsync(conversation.Id, "ReceiveChatMessage", dto);
        await _signalR.SendMessageToUserAsync(partnerId, "ReceiveNotification", new
        {
            Title = $"Message from {sender.FullName}",
            Message = message.Content.Length > 50 ? message.Content.Substring(0, 47) + "..." : message.Content,
            Type = NotificationType.ChatMessageReceived
        });

        return Result<MessageDTO>.Success(dto);
    }
}
