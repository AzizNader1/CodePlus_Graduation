using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Skills;

public class DeleteOfferedSkillCommandHandler : IRequestHandler<DeleteOfferedSkillCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteOfferedSkillCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(DeleteOfferedSkillCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<bool>.Failure("Unauthorized");

        var userSkill = await _context.UserSkills.FirstOrDefaultAsync(us => us.Id == command.Id && us.UserId == _currentUser.UserId && !us.IsDeleted, cancellationToken);
        if (userSkill == null)
            return Result<bool>.Failure("Offered skill not found.");

        userSkill.IsDeleted = true;
        userSkill.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
