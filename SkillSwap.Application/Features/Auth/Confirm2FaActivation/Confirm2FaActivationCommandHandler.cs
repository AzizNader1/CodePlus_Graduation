using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Auth;

public class Confirm2FaActivationCommandHandler : IRequestHandler<Confirm2FaActivationCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ITwoFactorService _twoFactorService;

    public Confirm2FaActivationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ITwoFactorService twoFactorService)
    {
        _context = context;
        _currentUser = currentUser;
        _twoFactorService = twoFactorService;
    }

    public async Task<Result<bool>> Handle(Confirm2FaActivationCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<bool>.Failure("Unauthorized");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.TwoFactorSecretKey))
            return Result<bool>.Failure("2FA setup not initiated.");

        var isValid = _twoFactorService.VerifyTotpCode(user.TwoFactorSecretKey, request.Code);
        if (!isValid)
            return Result<bool>.Failure("Invalid authentication code.");

        user.TwoFactorEnabled = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
