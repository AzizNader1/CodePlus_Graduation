using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Auth;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, Result<AuthResponseDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly IGoogleAuthService _googleAuth;
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public GoogleLoginCommandHandler(
        IApplicationDbContext context,
        IGoogleAuthService googleAuth,
        IIdentityService identityService,
        IJwtTokenGenerator jwtGenerator)
    {
        _context = context;
        _googleAuth = googleAuth;
        _identityService = identityService;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<Result<AuthResponseDTO>> Handle(GoogleLoginCommand command, CancellationToken cancellationToken)
    {
        var payload = await _googleAuth.ValidateIdTokenAsync(command.Request.IdToken);
        if (payload == null)
            return Result<AuthResponseDTO>.Failure("Invalid Google ID Token.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email || u.GoogleId == payload.GoogleId, cancellationToken);
        var effectiveName = !string.IsNullOrWhiteSpace(payload.Name) 
            ? payload.Name 
            : (!string.IsNullOrWhiteSpace(payload.Email) ? payload.Email.Split('@')[0] : "Google User");

        if (user == null)
        {
            user = new ApplicationUser
            {
                Email = payload.Email,
                UserName = payload.Email,
                FullName = effectiveName,
                AvatarUrl = payload.Picture,
                GoogleId = payload.GoogleId,
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
            user.GoogleId = payload.GoogleId;
            if (string.IsNullOrEmpty(user.FullName))
                user.FullName = effectiveName;
            if (string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(payload.Picture))
                user.AvatarUrl = payload.Picture;
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
