using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Sessions;

public class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, Result<ICollection<SwapSessionDTO>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSessionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ICollection<SwapSessionDTO>>> Handle(GetSessionsQuery query, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<ICollection<SwapSessionDTO>>.Failure("Please sign in to view your sessions.");

        var userId = _currentUser.UserId.Value;

        var q = _context.SwapSessions
            .Include(s => s.HostUser)
            .Include(s => s.ParticipantUser)
            .Where(s => (s.HostUserId == userId || s.ParticipantUserId == userId) && !s.IsDeleted);

        if (query.Status.HasValue)
            q = q.Where(s => s.Status == query.Status.Value);

        var list = await q.OrderByDescending(s => s.ScheduledStartTime).Select(s => new SwapSessionDTO
        {
            Id = s.Id,
            SwapRequestId = s.SwapRequestId,
            HostUserId = s.HostUserId,
            HostName = s.HostUser.FullName,
            HostAvatarUrl = s.HostUser.AvatarUrl,
            ParticipantUserId = s.ParticipantUserId,
            ParticipantName = s.ParticipantUser.FullName,
            ParticipantAvatarUrl = s.ParticipantUser.AvatarUrl,
            ScheduledStartTime = s.ScheduledStartTime,
            ScheduledEndTime = s.ScheduledEndTime,
            DurationMinutes = s.DurationMinutes,
            MeetingLink = s.MeetingLink,
            Status = s.Status,
            HostConfirmedCompleted = s.HostConfirmedCompleted,
            ParticipantConfirmedCompleted = s.ParticipantConfirmedCompleted,
            CompletedAt = s.CompletedAt,
            HasCurrentUserReviewed = s.Reviews.Any(r => r.ReviewerId == userId && !r.IsDeleted)
        }).ToListAsync(cancellationToken);

        return Result<ICollection<SwapSessionDTO>>.Success(list);
    }
}
