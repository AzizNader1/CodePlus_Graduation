using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Discovery;

public class SearchSkillsQueryHandler : IRequestHandler<SearchSkillsQuery, Result<PaginatedList<UserSkillDTO>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SearchSkillsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<UserSkillDTO>>> Handle(SearchSkillsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UserSkills
            .Include(us => us.Skill).ThenInclude(s => s.Category)
            .Include(us => us.User)
            .Where(us => us.IsActive && !us.IsDeleted && us.User.IsActive);

        if (_currentUser.UserId.HasValue)
        {
            query = query.Where(us => us.UserId != _currentUser.UserId.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(us => us.Skill.CategoryId == request.CategoryId.Value);
        }

        if (request.Level.HasValue)
        {
            query = query.Where(us => us.ProficiencyLevel == request.Level.Value);
        }

        if (request.MinRating.HasValue)
        {
            query = query.Where(us => us.User.AverageRating >= request.MinRating.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            query = query.Where(us => us.User.Location != null && us.User.Location.Contains(request.Location));
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(us =>
                us.Skill.Name.Contains(request.Keyword) ||
                (us.Description != null && us.Description.Contains(request.Keyword)) ||
                us.User.FullName.Contains(request.Keyword));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(us => us.User.AverageRating)
            .ThenByDescending(us => us.YearsOfExperience)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
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

        return Result<PaginatedList<UserSkillDTO>>.Success(
            new PaginatedList<UserSkillDTO>(items, totalCount, request.PageNumber, request.PageSize));
    }
}
