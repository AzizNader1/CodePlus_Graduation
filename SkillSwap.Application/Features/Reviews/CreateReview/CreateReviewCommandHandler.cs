using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Reviews;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<ReviewDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISignalRNotificationService _signalR;

    public CreateReviewCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ISignalRNotificationService signalR)
    {
        _context = context;
        _currentUser = currentUser;
        _signalR = signalR;
    }

    public async Task<Result<ReviewDTO>> Handle(CreateReviewCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<ReviewDTO>.Failure("Please sign in to leave a review.");

        var req = command.Request;
        var session = await _context.SwapSessions
            .Include(s => s.HostUser)
            .Include(s => s.ParticipantUser)
            .FirstOrDefaultAsync(s => s.Id == req.SessionId && !s.IsDeleted, cancellationToken);

        if (session == null)
            return Result<ReviewDTO>.Failure("We couldn't locate this swap session.");

        if (session.Status != SessionStatus.Completed)
            return Result<ReviewDTO>.Failure("Reviews can only be submitted once the session has been completed by both partners.");

        var reviewerId = _currentUser.UserId.Value;
        if (session.HostUserId != reviewerId && session.ParticipantUserId != reviewerId)
            return Result<ReviewDTO>.Failure("You can only review swap sessions that you participated in.");

        var revieweeId = session.HostUserId == reviewerId ? session.ParticipantUserId : session.HostUserId;

        var existing = await _context.Reviews
            .FirstOrDefaultAsync(r => r.SessionId == session.Id && r.ReviewerId == reviewerId && !r.IsDeleted, cancellationToken);

        if (existing != null)
            return Result<ReviewDTO>.Failure("You have already submitted your review for this session. Thank you for your feedback!");

        var review = new Review
        {
            SessionId = session.Id,
            ReviewerId = reviewerId,
            RevieweeId = revieweeId,
            OverallRating = Math.Clamp(req.OverallRating, 1, 5),
            PunctualityScore = Math.Clamp(req.PunctualityScore, 1, 5),
            CommunicationScore = Math.Clamp(req.CommunicationScore, 1, 5),
            KnowledgeScore = Math.Clamp(req.KnowledgeScore, 1, 5),
            Comment = req.Comment
        };

        _context.Reviews.Add(review);

        var targetUser = await _context.Users.FindAsync(new object[] { revieweeId }, cancellationToken);
        if (targetUser != null)
        {
            var pastRatings = await _context.Reviews
                .Where(r => r.RevieweeId == revieweeId && !r.IsDeleted)
                .Select(r => r.OverallRating)
                .ToListAsync(cancellationToken);

            pastRatings.Add(review.OverallRating);

            targetUser.TotalReviewsCount = pastRatings.Count;
            targetUser.AverageRating = Math.Round(pastRatings.Average(), 1);
        }

        var reviewer = session.HostUserId == reviewerId ? session.HostUser : session.ParticipantUser;
        var notif = new Notification
        {
            UserId = revieweeId,
            Title = "New Review Received",
            Message = $"{reviewer.FullName} left you a {review.OverallRating}-star review!",
            Type = NotificationType.ReviewReceived,
            TargetReferenceId = session.Id
        };
        _context.Notifications.Add(notif);

        await _context.SaveChangesAsync(cancellationToken);

        await _signalR.SendMessageToUserAsync(revieweeId, "ReceiveNotification", new
        {
            notif.Id,
            notif.Title,
            notif.Message,
            notif.Type
        });

        return Result<ReviewDTO>.Success(new ReviewDTO
        {
            Id = review.Id,
            SessionId = review.SessionId,
            ReviewerId = review.ReviewerId,
            ReviewerName = reviewer.FullName,
            ReviewerAvatarUrl = reviewer.AvatarUrl,
            OverallRating = review.OverallRating,
            PunctualityScore = review.PunctualityScore,
            CommunicationScore = review.CommunicationScore,
            KnowledgeScore = review.KnowledgeScore,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        });
    }
}
