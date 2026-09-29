using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Skills;

public class AddOfferedSkillCommandHandler : IRequestHandler<AddOfferedSkillCommand, Result<UserSkillDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddOfferedSkillCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<UserSkillDTO>> Handle(AddOfferedSkillCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<UserSkillDTO>.Failure("Unauthorized");

        var skill = await _context.Skills.Include(s => s.Category).FirstOrDefaultAsync(s => s.Id == command.Request.SkillId, cancellationToken);
        if (skill == null)
            return Result<UserSkillDTO>.Failure("Skill not found in catalog.");

        var existing = await _context.UserSkills.FirstOrDefaultAsync(us => us.UserId == _currentUser.UserId && us.SkillId == skill.Id && !us.IsDeleted, cancellationToken);
        if (existing != null)
            return Result<UserSkillDTO>.Failure("You have already added this skill to your offered portfolio.");

        var userSkill = new UserSkill
        {
            UserId = _currentUser.UserId.Value,
            SkillId = skill.Id,
            ProficiencyLevel = command.Request.ProficiencyLevel,
            YearsOfExperience = command.Request.YearsOfExperience,
            PortfolioUrl = command.Request.PortfolioUrl,
            Description = command.Request.Description
        };

        _context.UserSkills.Add(userSkill);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<UserSkillDTO>.Success(new UserSkillDTO
        {
            Id = userSkill.Id,
            SkillId = skill.Id,
            SkillName = skill.Name,
            CategoryName = skill.Category.Name,
            ProficiencyLevel = userSkill.ProficiencyLevel,
            YearsOfExperience = userSkill.YearsOfExperience,
            PortfolioUrl = userSkill.PortfolioUrl,
            Description = userSkill.Description
        });
    }
}
