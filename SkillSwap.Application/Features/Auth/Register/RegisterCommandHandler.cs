using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserSummaryDTO>>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public RegisterCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task<Result<UserSummaryDTO>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var existingUser = await _context.Users.AnyAsync(u => u.Email == req.Email, cancellationToken);
        if (existingUser)
        {
            return Result<UserSummaryDTO>.Failure("Email address is already in use.");
        }

        var (result, userId) = await _identityService.CreateUserAsync(
            req.Email, req.Password, req.FullName, req.Location, req.TimeZone);

        if (!result.IsSuccess)
        {
            return Result<UserSummaryDTO>.Failure(result.Errors);
        }

        var userSummary = new UserSummaryDTO
        {
            Id = userId,
            FullName = req.FullName,
            Email = req.Email,
            AverageRating = 0.0,
            TotalSwapsCompleted = 0,
            Roles = new[] { "User" }
        };

        return Result<UserSummaryDTO>.Success(userSummary);
    }
}
