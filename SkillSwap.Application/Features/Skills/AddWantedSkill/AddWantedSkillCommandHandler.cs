using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Skills;

public class AddWantedSkillCommandHandler : IRequestHandler<AddWantedSkillCommand, Result<UserDesiredSkillDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddWantedSkillCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<UserDesiredSkillDTO>> Handle(AddWantedSkillCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<UserDesiredSkillDTO>.Failure("Unauthorized");

        var skill = await _context.Skills.Include(s => s.Category).FirstOrDefaultAsync(s => s.Id == command.Request.SkillId, cancellationToken);
        if (skill == null)
            return Result<UserDesiredSkillDTO>.Failure("Skill not found in catalog.");

        var existing = await _context.UserDesiredSkills.FirstOrDefaultAsync(ds => ds.UserId == _currentUser.UserId && ds.SkillId == skill.Id, cancellationToken);
        if (existing != null)
            return Result<UserDesiredSkillDTO>.Failure("You have already added this skill to your learning wishlist.");

        var desiredSkill = new UserDesiredSkill
        {
            UserId = _currentUser.UserId.Value,
            SkillId = skill.Id,
            TargetLevel = command.Request.TargetLevel,
            Priority = command.Request.Priority,
            Description = command.Request.Description
        };

        _context.UserDesiredSkills.Add(desiredSkill);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<UserDesiredSkillDTO>.Success(new UserDesiredSkillDTO
        {
            Id = desiredSkill.Id,
            SkillId = skill.Id,
            SkillName = skill.Name,
            CategoryName = skill.Category.Name,
            TargetLevel = desiredSkill.TargetLevel,
            Priority = desiredSkill.Priority,
            Description = desiredSkill.Description
        });
    }
}
