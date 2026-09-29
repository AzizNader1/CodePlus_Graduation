using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Auth;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IEmailService emailService)
    {
        _context = context;
        _identityService = identityService;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == command.Request.Email, cancellationToken);
        if (user != null)
        {
            var resetToken = await _identityService.GeneratePasswordResetTokenAsync(user);
            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                    <h2 style='color: #6C5CE7;'>Skill Swap Password Reset</h2>
                    <p>Hello {user.FullName},</p>
                    <p>We received a request to reset your password. Use the token below to complete the reset process:</p>
                    <div style='background: #F0EDFF; padding: 15px; border-radius: 8px; font-size: 18px; font-weight: bold; letter-spacing: 2px; text-align: center;'>
                        {resetToken}
                    </div>
                    <p style='color: #777; margin-top: 20px;'>If you did not make this request, you can safely ignore this email.</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email!, "Reset Your Skill Swap Password", emailBody, cancellationToken);
        }

        return Result<bool>.Success(true);
    }
}
