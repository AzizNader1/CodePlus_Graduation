using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Admin;

public class GetAdminReportsQueryHandler : IRequestHandler<GetAdminReportsQuery, Result<ICollection<ReportDTO>>>
{
    private readonly IApplicationDbContext _context;

    public GetAdminReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ICollection<ReportDTO>>> Handle(GetAdminReportsQuery query, CancellationToken cancellationToken)
    {
        var reports = await _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.ReportedUser)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReportDTO
            {
                Id = r.Id,
                ReporterId = r.ReporterId,
                ReporterName = r.Reporter.FullName,
                ReportedUserId = r.ReportedUserId,
                ReportedUserName = r.ReportedUser.FullName,
                SessionId = r.SessionId,
                ReasonCategory = r.ReasonCategory,
                Details = r.Details,
                Status = r.Status,
                AdminNotes = r.AdminNotes,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<ICollection<ReportDTO>>.Success(reports);
    }
}
