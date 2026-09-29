using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Profile;

public class SetAvailabilityCommandHandler : IRequestHandler<SetAvailabilityCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SetAvailabilityCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(SetAvailabilityCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<bool>.Failure("Unauthorized");

        var existing = await _context.UserAvailabilities
            .Where(a => a.UserId == _currentUser.UserId)
            .ToListAsync(cancellationToken);

        _context.UserAvailabilities.RemoveRange(existing);

        foreach (var slot in command.Slots)
        {
            _context.UserAvailabilities.Add(new UserAvailability
            {
                UserId = _currentUser.UserId.Value,
                DayOfWeek = slot.DayOfWeek,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                IsRecurring = slot.IsRecurring
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
