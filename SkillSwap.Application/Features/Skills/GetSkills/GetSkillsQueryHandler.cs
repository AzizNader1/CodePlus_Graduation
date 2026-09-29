using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Skills;

public class GetSkillsQueryHandler : IRequestHandler<GetSkillsQuery, Result<ICollection<SkillDTO>>>
{
    private readonly IApplicationDbContext _context;

    public GetSkillsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ICollection<SkillDTO>>> Handle(GetSkillsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Skills
            .Include(s => s.Category)
            .Where(s => s.IsActive && !s.IsDeleted);

        if (request.CategoryId.HasValue)
        {
            query = query.Where(s => s.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(s => s.Name.Contains(request.Search) || (s.Description != null && s.Description.Contains(request.Search)));
        }

        var skills = await query
            .OrderBy(s => s.Name)
            .Take(50)
            .Select(s => new SkillDTO
            {
                Id = s.Id,
                CategoryId = s.CategoryId,
                CategoryName = s.Category.Name,
                Name = s.Name,
                Description = s.Description
            })
            .ToListAsync(cancellationToken);

        return Result<ICollection<SkillDTO>>.Success(skills);
    }
}
