using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _identityService = identityService;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponseDTO>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == req.Email, cancellationToken);
        if (user == null || !user.IsActive)
        {
            return Result<AuthResponseDTO>.Failure("Invalid email or password.");
        }

        var check = await _identityService.CheckPasswordAsync(user, req.Password);
        if (!check.IsSuccess)
        {
            return Result<AuthResponseDTO>.Failure("Invalid email or password.");
        }

        if (user.TwoFactorEnabled)
        {
            var tempToken = Guid.NewGuid().ToString("N");
            user.RefreshToken = tempToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(10);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<AuthResponseDTO>.Success(new AuthResponseDTO
            {
                RequiresTwoFactor = true,
                TwoFactorToken = tempToken
            });
        }

        var roles = await _identityService.GetRolesAsync(user);
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<AuthResponseDTO>.Success(new AuthResponseDTO
        {
            RequiresTwoFactor = false,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            User = new UserSummaryDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                AvatarUrl = user.AvatarUrl,
                AverageRating = user.AverageRating,
                TotalSwapsCompleted = user.TotalSwapsCompleted,
                Roles = roles
            }
        });
    }
}
