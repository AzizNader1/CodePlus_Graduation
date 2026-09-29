using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Profile;

public class GetPublicUserProfileQueryHandler : IRequestHandler<GetPublicUserProfileQuery, Result<PublicUserProfileDTO>>
{
    private readonly IApplicationDbContext _context;

    public GetPublicUserProfileQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PublicUserProfileDTO>> Handle(GetPublicUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.SkillsOffered.Where(s => !s.IsDeleted)).ThenInclude(s => s.Skill).ThenInclude(sk => sk.Category)
            .Include(u => u.SkillsWanted).ThenInclude(w => w.Skill).ThenInclude(sk => sk.Category)
            .Include(u => u.Availabilities)
            .Include(u => u.ReviewsReceived.Where(r => !r.IsDeleted)).ThenInclude(r => r.Reviewer)
            .FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive, cancellationToken);

        if (user == null)
            return Result<PublicUserProfileDTO>.Failure("User not found.");

        var dto = new PublicUserProfileDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            Location = user.Location,
            AverageRating = user.AverageRating,
            TotalReviewsCount = user.TotalReviewsCount,
            TotalSwapsCompleted = user.TotalSwapsCompleted,
            SkillsOffered = user.SkillsOffered.Select(s => new UserSkillDTO
            {
                Id = s.Id,
                SkillId = s.SkillId,
                SkillName = s.Skill.Name,
                CategoryName = s.Skill.Category.Name,
                ProficiencyLevel = s.ProficiencyLevel,
                YearsOfExperience = s.YearsOfExperience,
                PortfolioUrl = s.PortfolioUrl,
                Description = s.Description
            }).ToList(),
            SkillsWanted = user.SkillsWanted.Select(w => new UserDesiredSkillDTO
            {
                Id = w.Id,
                SkillId = w.SkillId,
                SkillName = w.Skill.Name,
                CategoryName = w.Skill.Category.Name,
                TargetLevel = w.TargetLevel,
                Priority = w.Priority,
                Description = w.Description
            }).ToList(),
            Availabilities = user.Availabilities.Select(a => new UserAvailabilityDTO
            {
                Id = a.Id,
                DayOfWeek = a.DayOfWeek,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                IsRecurring = a.IsRecurring
            }).ToList(),
            RecentReviews = user.ReviewsReceived.OrderByDescending(r => r.CreatedAt).Take(5).Select(r => new ReviewDTO
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
            }).ToList()
        };

        return Result<PublicUserProfileDTO>.Success(dto);
    }
}
