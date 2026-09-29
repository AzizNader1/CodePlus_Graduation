using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Admin;

public class ResolveReportCommandHandler : IRequestHandler<ResolveReportCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ResolveReportCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(ResolveReportCommand command, CancellationToken cancellationToken)
    {
        var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == command.ReportId, cancellationToken);
        if (report == null)
            return Result<bool>.Failure("Report not found.");

        report.Status = command.Request.Status;
        report.AdminNotes = command.Request.AdminNotes;
        report.ResolvedAt = DateTime.UtcNow;
        report.ResolvedByAdminId = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
