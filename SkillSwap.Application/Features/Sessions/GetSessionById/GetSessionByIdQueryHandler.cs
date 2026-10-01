using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Sessions;

public class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, Result<SwapSessionDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSessionByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<SwapSessionDTO>> Handle(GetSessionByIdQuery query, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<SwapSessionDTO>.Failure("Please sign in to view this session.");

        var s = await _context.SwapSessions
            .Include(ss => ss.HostUser)
            .Include(ss => ss.ParticipantUser)
            .FirstOrDefaultAsync(ss => ss.Id == query.Id && !ss.IsDeleted, cancellationToken);

        if (s == null)
            return Result<SwapSessionDTO>.Failure("We couldn't locate this session. It may have been completed or removed.");

        if (s.HostUserId != _currentUser.UserId.Value && s.ParticipantUserId != _currentUser.UserId.Value)
            return Result<SwapSessionDTO>.Failure("You do not have permission to access this session.");

        return Result<SwapSessionDTO>.Success(new SwapSessionDTO
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
            HasCurrentUserReviewed = await _context.Reviews.AnyAsync(r => r.SessionId == s.Id && r.ReviewerId == _currentUser.UserId.Value && !r.IsDeleted, cancellationToken)
        });
    }
}
