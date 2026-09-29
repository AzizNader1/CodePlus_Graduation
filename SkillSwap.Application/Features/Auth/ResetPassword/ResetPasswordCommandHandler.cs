using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Auth;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == req.Email, cancellationToken);
        if (user == null)
            return Result<bool>.Failure("Invalid reset attempt.");

        var result = await _identityService.ResetPasswordAsync(user, req.Token, req.NewPassword);
        if (!result.IsSuccess)
            return Result<bool>.Failure(result.Errors);

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
