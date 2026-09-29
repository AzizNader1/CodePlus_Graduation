using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Auth;

public class AppleLoginCommandHandler : IRequestHandler<AppleLoginCommand, Result<AuthResponseDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAppleAuthService _appleAuth;
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public AppleLoginCommandHandler(
        IApplicationDbContext context,
        IAppleAuthService appleAuth,
        IIdentityService identityService,
        IJwtTokenGenerator jwtGenerator)
    {
        _context = context;
        _appleAuth = appleAuth;
        _identityService = identityService;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<Result<AuthResponseDTO>> Handle(AppleLoginCommand command, CancellationToken cancellationToken)
    {
        var payload = await _appleAuth.ValidateIdentityTokenAsync(command.Request.IdentityToken);
        if (payload == null)
            return Result<AuthResponseDTO>.Failure("Invalid Apple Identity Token.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.AppleId == payload.AppleId || u.Email == payload.Email, cancellationToken);
        if (user == null)
        {
            user = new ApplicationUser
            {
                Email = payload.Email,
                UserName = payload.Email,
                FullName = command.Request.FullName ?? "Apple User",
                AppleId = payload.AppleId,
                EmailConfirmed = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
            await _identityService.AddToRoleAsync(user, "User");
        }
        else
        {
            if (string.IsNullOrEmpty(user.SecurityStamp))
                user.SecurityStamp = Guid.NewGuid().ToString();
            user.AppleId = payload.AppleId;
        }

        var roles = await _identityService.GetRolesAsync(user);
        var accessToken = _jwtGenerator.GenerateAccessToken(user, roles);
        var refreshToken = _jwtGenerator.GenerateRefreshToken();

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
