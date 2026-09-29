using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Profile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<UserProfileDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetProfileQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<UserProfileDTO>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<UserProfileDTO>.Failure("Unauthorized");

        var user = await _context.Users
            .Include(u => u.SkillsOffered.Where(s => !s.IsDeleted)).ThenInclude(s => s.Skill).ThenInclude(sk => sk.Category)
            .Include(u => u.SkillsWanted).ThenInclude(w => w.Skill).ThenInclude(sk => sk.Category)
            .Include(u => u.Availabilities)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        if (user == null)
            return Result<UserProfileDTO>.Failure("User profile not found.");

        var dto = new UserProfileDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            Location = user.Location,
            TimeZone = user.TimeZone,
            AverageRating = user.AverageRating,
            TotalReviewsCount = user.TotalReviewsCount,
            TotalSwapsCompleted = user.TotalSwapsCompleted,
            TwoFactorEnabled = user.TwoFactorEnabled,
            CreatedAt = user.CreatedAt,
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
            }).ToList()
        };

        return Result<UserProfileDTO>.Success(dto);
    }
}
