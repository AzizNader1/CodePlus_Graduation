using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Discovery;

public class GetRecommendedMatchesQueryHandler : IRequestHandler<GetRecommendedMatchesQuery, Result<ICollection<SwapMatchDTO>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetRecommendedMatchesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ICollection<SwapMatchDTO>>> Handle(GetRecommendedMatchesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<ICollection<SwapMatchDTO>>.Failure("Unauthorized");

        var currentUserId = _currentUser.UserId.Value;

        var myOfferedSkillIds = await _context.UserSkills
            .Where(us => us.UserId == currentUserId && us.IsActive && !us.IsDeleted)
            .Select(us => us.SkillId)
            .ToListAsync(cancellationToken);

        var myWantedSkillIds = await _context.UserDesiredSkills
            .Where(ds => ds.UserId == currentUserId)
            .Select(ds => ds.SkillId)
            .ToListAsync(cancellationToken);

        if (!myOfferedSkillIds.Any() || !myWantedSkillIds.Any())
        {
            return Result<ICollection<SwapMatchDTO>>.Success(new List<SwapMatchDTO>());
        }

        var candidates = await _context.Users
            .Where(u => u.Id != currentUserId && u.IsActive)
            .Where(u => u.SkillsOffered.Any(so => myWantedSkillIds.Contains(so.SkillId) && so.IsActive && !so.IsDeleted))
            .Where(u => u.SkillsWanted.Any(sw => myOfferedSkillIds.Contains(sw.SkillId)))
            .Include(u => u.SkillsOffered.Where(so => so.IsActive && !so.IsDeleted)).ThenInclude(so => so.Skill)
            .Include(u => u.SkillsWanted).ThenInclude(sw => sw.Skill)
            .Take(20)
            .ToListAsync(cancellationToken);

        var matches = new List<SwapMatchDTO>();

        foreach (var partner in candidates)
        {
            var theyTeach = partner.SkillsOffered.FirstOrDefault(so => myWantedSkillIds.Contains(so.SkillId));
            var youTeach = partner.SkillsWanted.FirstOrDefault(sw => myOfferedSkillIds.Contains(sw.SkillId));

            if (theyTeach != null && youTeach != null)
            {
                int score = 80 + (int)(partner.AverageRating * 4);
                if (score > 100) score = 100;

                matches.Add(new SwapMatchDTO
                {
                    PartnerUser = new PublicUserProfileDTO
                    {
                        Id = partner.Id,
                        FullName = partner.FullName,
                        Bio = partner.Bio,
                        AvatarUrl = partner.AvatarUrl,
                        Location = partner.Location,
                        AverageRating = partner.AverageRating,
                        TotalReviewsCount = partner.TotalReviewsCount,
                        TotalSwapsCompleted = partner.TotalSwapsCompleted
                    },
                    TheyTeachSkill = theyTeach.Skill.Name,
                    YouTeachSkill = youTeach.Skill.Name,
                    MatchCompatibilityScore = score
                });
            }
        }

        return Result<ICollection<SwapMatchDTO>>.Success(matches.OrderByDescending(m => m.MatchCompatibilityScore).ToList());
    }
}
