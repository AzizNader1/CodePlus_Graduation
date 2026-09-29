using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Discovery;

public class GetHomeFeedQueryHandler : IRequestHandler<GetHomeFeedQuery, Result<HomeFeedDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMediator _mediator;

    public GetHomeFeedQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<Result<HomeFeedDTO>> Handle(GetHomeFeedQuery request, CancellationToken cancellationToken)
    {
        var feed = new HomeFeedDTO();

        feed.FeaturedCategories = await _context.Categories
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .Take(6)
            .Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IconUrl = c.IconUrl,
                DisplayOrder = c.DisplayOrder,
                SkillsCount = c.Skills.Count(s => s.IsActive && !s.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        var topSwappersQuery = _context.Users.Where(u => u.IsActive);
        if (_currentUser.UserId.HasValue)
        {
            topSwappersQuery = topSwappersQuery.Where(u => u.Id != _currentUser.UserId.Value);
        }

        feed.TopRatedSwappers = await topSwappersQuery
            .OrderByDescending(u => u.AverageRating)
            .ThenByDescending(u => u.TotalSwapsCompleted)
            .Take(5)
            .Select(u => new PublicUserProfileDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                Bio = u.Bio,
                AvatarUrl = u.AvatarUrl,
                Location = u.Location,
                AverageRating = u.AverageRating,
                TotalReviewsCount = u.TotalReviewsCount,
                TotalSwapsCompleted = u.TotalSwapsCompleted
            })
            .ToListAsync(cancellationToken);

        feed.TrendingSkills = await _context.UserSkills
            .Include(us => us.Skill).ThenInclude(s => s.Category)
            .Where(us => us.IsActive && !us.IsDeleted && us.User.IsActive)
            .OrderByDescending(us => us.CreatedAt)
            .Take(8)
            .Select(us => new UserSkillDTO
            {
                Id = us.Id,
                SkillId = us.SkillId,
                SkillName = us.Skill.Name,
                CategoryName = us.Skill.Category.Name,
                ProficiencyLevel = us.ProficiencyLevel,
                YearsOfExperience = us.YearsOfExperience,
                PortfolioUrl = us.PortfolioUrl,
                Description = us.Description
            })
            .ToListAsync(cancellationToken);

        if (_currentUser.UserId.HasValue)
        {
            var matchResult = await _mediator.Send(new GetRecommendedMatchesQuery(), cancellationToken);
            if (matchResult.IsSuccess)
            {
                feed.RecommendedForYou = matchResult.Value.Take(4).ToList();
            }
        }

        return Result<HomeFeedDTO>.Success(feed);
    }
}
