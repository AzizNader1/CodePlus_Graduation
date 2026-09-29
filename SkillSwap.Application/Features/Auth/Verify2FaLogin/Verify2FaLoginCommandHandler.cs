using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Auth;

public class Verify2FaLoginCommandHandler : IRequestHandler<Verify2FaLoginCommand, Result<AuthResponseDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ITwoFactorService _twoFactorService;

    public Verify2FaLoginCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IJwtTokenGenerator jwtTokenGenerator,
        ITwoFactorService twoFactorService)
    {
        _context = context;
        _identityService = identityService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _twoFactorService = twoFactorService;
    }

    public async Task<Result<AuthResponseDTO>> Handle(Verify2FaLoginCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == req.TwoFactorToken, cancellationToken);
        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Result<AuthResponseDTO>.Failure("Invalid or expired 2FA login session.");
        }

        bool isCodeValid = false;
        if (!string.IsNullOrEmpty(user.TwoFactorSecretKey))
        {
            isCodeValid = _twoFactorService.VerifyTotpCode(user.TwoFactorSecretKey, req.Code);
        }

        if (!isCodeValid && !string.IsNullOrEmpty(user.TwoFactorRecoveryCodes))
        {
            var codes = user.TwoFactorRecoveryCodes.Split(';').ToList();
            if (codes.Contains(req.Code))
            {
                codes.Remove(req.Code);
                user.TwoFactorRecoveryCodes = string.Join(";", codes);
                isCodeValid = true;
            }
        }

        if (!isCodeValid)
        {
            return Result<AuthResponseDTO>.Failure("Invalid 2FA code.");
        }

        var roles = await _identityService.GetRolesAsync(user);
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<AuthResponseDTO>.Success(new AuthResponseDTO
        {
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
