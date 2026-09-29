using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Chat;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, Result<PaginatedList<MessageDTO>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMessagesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<MessageDTO>>> Handle(GetMessagesQuery query, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<PaginatedList<MessageDTO>>.Failure("Unauthorized");

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == query.ConversationId && !c.IsDeleted, cancellationToken);

        if (conversation == null)
            return Result<PaginatedList<MessageDTO>>.Failure("Conversation not found.");

        if (conversation.UserOneId != _currentUser.UserId.Value && conversation.UserTwoId != _currentUser.UserId.Value)
            return Result<PaginatedList<MessageDTO>>.Failure("Forbidden");

        var q = _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == query.ConversationId);

        var total = await q.CountAsync(cancellationToken);

        var messages = await q
            .OrderByDescending(m => m.SentAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(m => new MessageDTO
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                SenderName = m.Sender.FullName,
                Content = m.Content,
                AttachmentUrl = m.AttachmentUrl,
                IsRead = m.IsRead,
                SentAt = m.SentAt
            })
            .ToListAsync(cancellationToken);

        var unread = await _context.Messages
            .Where(m => m.ConversationId == query.ConversationId && m.SenderId != _currentUser.UserId.Value && !m.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Any())
        {
            foreach (var msg in unread)
            {
                msg.IsRead = true;
                msg.ReadAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<PaginatedList<MessageDTO>>.Success(
            new PaginatedList<MessageDTO>(messages.OrderBy(m => m.SentAt).ToList(), total, query.PageNumber, query.PageSize));
    }
}
