using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Reviews;

public class GetUserReviewsQueryHandler : IRequestHandler<GetUserReviewsQuery, Result<PaginatedList<ReviewDTO>>>
{
    private readonly IApplicationDbContext _context;

    public GetUserReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<ReviewDTO>>> Handle(GetUserReviewsQuery query, CancellationToken cancellationToken)
    {
        var q = _context.Reviews
            .Include(r => r.Reviewer)
            .Where(r => r.RevieweeId == query.UserId && !r.IsDeleted);

        var total = await q.CountAsync(cancellationToken);

        var list = await q.OrderByDescending(r => r.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new ReviewDTO
            {
                Id = r.Id,
                SessionId = r.SessionId,
                ReviewerId = r.ReviewerId,
                ReviewerName = r.Reviewer.FullName,
                ReviewerAvatarUrl = r.Reviewer.AvatarUrl,
                OverallRating = r.OverallRating,
                PunctualityScore = r.PunctualityScore,
                CommunicationScore = r.CommunicationScore,
                KnowledgeScore = r.KnowledgeScore,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<PaginatedList<ReviewDTO>>.Success(
            new PaginatedList<ReviewDTO>(list, total, query.PageNumber, query.PageSize));
    }
}
