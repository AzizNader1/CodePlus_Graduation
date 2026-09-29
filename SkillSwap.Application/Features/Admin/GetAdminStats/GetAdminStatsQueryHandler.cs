using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Admin;

public class GetAdminStatsQueryHandler : IRequestHandler<GetAdminStatsQuery, Result<AdminDashboardStatsDTO>>
{
    private readonly IApplicationDbContext _context;

    public GetAdminStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AdminDashboardStatsDTO>> Handle(GetAdminStatsQuery query, CancellationToken cancellationToken)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var activeSwaps = await _context.SwapRequests.CountAsync(sr => sr.Status == SwapRequestStatus.Accepted, cancellationToken);
        var completedSessions = await _context.SwapSessions.CountAsync(ss => ss.Status == SessionStatus.Completed, cancellationToken);
        var openReports = await _context.Reports.CountAsync(r => r.Status == ReportStatus.Pending, cancellationToken);
        var totalSkills = await _context.UserSkills.CountAsync(us => us.IsActive && !us.IsDeleted, cancellationToken);
        var totalVolume = await _context.PaymentTransactions.Where(p => p.Status == PaymentStatus.Succeeded).SumAsync(p => p.Amount, cancellationToken);

        return Result<AdminDashboardStatsDTO>.Success(new AdminDashboardStatsDTO
        {
            TotalRegisteredUsers = totalUsers,
            ActiveSwapsCount = activeSwaps,
            CompletedSessionsCount = completedSessions,
            OpenReportsCount = openReports,
            TotalSkillsListed = totalSkills,
            TotalPaymentVolume = totalVolume
        });
    }
}
