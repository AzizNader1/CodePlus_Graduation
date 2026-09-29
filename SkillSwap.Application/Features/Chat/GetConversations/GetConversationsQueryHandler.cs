using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Chat;

public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, Result<ICollection<ConversationDTO>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetConversationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ICollection<ConversationDTO>>> Handle(GetConversationsQuery query, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<ICollection<ConversationDTO>>.Failure("Unauthorized");

        var userId = _currentUser.UserId.Value;

        var conversations = await _context.Conversations
            .Include(c => c.UserOne)
            .Include(c => c.UserTwo)
            .Include(c => c.Messages)
            .Where(c => (c.UserOneId == userId || c.UserTwoId == userId) && !c.IsDeleted)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync(cancellationToken);

        var list = conversations.Select(c =>
        {
            var partner = c.UserOneId == userId ? c.UserTwo : c.UserOne;
            var lastMsg = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
            var unread = c.Messages.Count(m => m.SenderId != userId && !m.IsRead);

            return new ConversationDTO
            {
                Id = c.Id,
                PartnerId = partner.Id,
                PartnerName = partner.FullName,
                PartnerAvatarUrl = partner.AvatarUrl,
                LastMessageContent = lastMsg?.Content,
                LastMessageAt = c.LastMessageAt,
                UnreadCount = unread
            };
        }).ToList();

        return Result<ICollection<ConversationDTO>>.Success(list);
    }
}
