using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Profile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserProfileDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMediator _mediator;

    public UpdateProfileCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<Result<UserProfileDTO>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<UserProfileDTO>.Failure("Unauthorized");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);
        if (user == null)
            return Result<UserProfileDTO>.Failure("User not found.");

        user.FullName = command.Request.FullName;
        user.Bio = command.Request.Bio;
        user.Location = command.Request.Location;
        user.TimeZone = command.Request.TimeZone;

        await _context.SaveChangesAsync(cancellationToken);
        return await _mediator.Send(new GetProfileQuery(), cancellationToken);
    }
}
