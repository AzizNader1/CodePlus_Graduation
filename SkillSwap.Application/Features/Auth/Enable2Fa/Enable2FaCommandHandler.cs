using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Auth;

public class Enable2FaCommandHandler : IRequestHandler<Enable2FaCommand, Result<Enable2FaResponseDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ITwoFactorService _twoFactorService;

    public Enable2FaCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ITwoFactorService twoFactorService)
    {
        _context = context;
        _currentUser = currentUser;
        _twoFactorService = twoFactorService;
    }

    public async Task<Result<Enable2FaResponseDTO>> Handle(Enable2FaCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<Enable2FaResponseDTO>.Failure("Unauthorized");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);
        if (user == null)
            return Result<Enable2FaResponseDTO>.Failure("User not found");

        var secret = _twoFactorService.GenerateSecretKey();
        var uri = _twoFactorService.GenerateQrCodeUri(user.Email!, secret);
        var recoveryCodes = _twoFactorService.GenerateRecoveryCodes().ToList();

        user.TwoFactorSecretKey = secret;
        user.TwoFactorRecoveryCodes = string.Join(";", recoveryCodes);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Enable2FaResponseDTO>.Success(new Enable2FaResponseDTO
        {
            SecretKey = secret,
            QrCodeUri = uri,
            RecoveryCodes = recoveryCodes
        });
    }
}
