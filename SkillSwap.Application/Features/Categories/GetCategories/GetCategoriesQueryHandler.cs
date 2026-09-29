using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Categories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<ICollection<CategoryDTO>>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ICollection<CategoryDTO>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
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

        return Result<ICollection<CategoryDTO>>.Success(categories);
    }
}
