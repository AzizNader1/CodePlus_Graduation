using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Notifications;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<PaginatedList<NotificationDTO>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetNotificationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<NotificationDTO>>> Handle(GetNotificationsQuery query, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<PaginatedList<NotificationDTO>>.Failure("Unauthorized");

        var q = _context.Notifications
            .Where(n => n.UserId == _currentUser.UserId.Value);

        if (query.UnreadOnly == true)
            q = q.Where(n => !n.IsRead);

        var total = await q.CountAsync(cancellationToken);

        var items = await q.OrderByDescending(n => n.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(n => new NotificationDTO
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                TargetUrl = n.TargetUrl,
                TargetReferenceId = n.TargetReferenceId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<PaginatedList<NotificationDTO>>.Success(
            new PaginatedList<NotificationDTO>(items, total, query.PageNumber, query.PageSize));
    }
}
