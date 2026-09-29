using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Notifications;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkNotificationAsReadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(MarkNotificationAsReadCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<bool>.Failure("Unauthorized");

        var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == command.Id && n.UserId == _currentUser.UserId.Value, cancellationToken);
        if (notif == null)
            return Result<bool>.Failure("Notification not found.");

        notif.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
