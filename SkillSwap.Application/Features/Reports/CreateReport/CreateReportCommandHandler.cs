using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Reports;

public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateReportCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(CreateReportCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<bool>.Failure("Unauthorized");

        var reporterId = _currentUser.UserId.Value;
        if (reporterId == command.Request.ReportedUserId)
            return Result<bool>.Failure("You cannot report yourself.");

        var report = new Report
        {
            ReporterId = reporterId,
            ReportedUserId = command.Request.ReportedUserId,
            SessionId = command.Request.SessionId,
            ReasonCategory = command.Request.ReasonCategory,
            Details = command.Request.Details,
            Status = ReportStatus.Pending
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
