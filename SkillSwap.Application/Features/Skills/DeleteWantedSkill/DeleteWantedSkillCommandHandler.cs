using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Skills;

public class DeleteWantedSkillCommandHandler : IRequestHandler<DeleteWantedSkillCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteWantedSkillCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(DeleteWantedSkillCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<bool>.Failure("Unauthorized");

        var desired = await _context.UserDesiredSkills.FirstOrDefaultAsync(ds => ds.Id == command.Id && ds.UserId == _currentUser.UserId, cancellationToken);
        if (desired == null)
            return Result<bool>.Failure("Skill not found.");

        _context.UserDesiredSkills.Remove(desired);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
