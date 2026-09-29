using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.SwapRequests;

public class GetOutgoingSwapRequestsQueryHandler : IRequestHandler<GetOutgoingSwapRequestsQuery, Result<ICollection<SwapRequestDTO>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOutgoingSwapRequestsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ICollection<SwapRequestDTO>>> Handle(GetOutgoingSwapRequestsQuery query, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<ICollection<SwapRequestDTO>>.Failure("Unauthorized");

        var q = _context.SwapRequests
            .Include(sr => sr.Requester)
            .Include(sr => sr.Receiver)
            .Include(sr => sr.OfferedSkill)
            .Include(sr => sr.RequestedSkill)
            .Where(sr => sr.RequesterId == _currentUser.UserId.Value && !sr.IsDeleted);

        if (query.Status.HasValue)
            q = q.Where(sr => sr.Status == query.Status.Value);

        var list = await q.OrderByDescending(sr => sr.CreatedAt).Select(sr => new SwapRequestDTO
        {
            Id = sr.Id,
            RequesterId = sr.RequesterId,
            RequesterName = sr.Requester.FullName,
            RequesterAvatarUrl = sr.Requester.AvatarUrl,
            RequesterRating = sr.Requester.AverageRating,
            ReceiverId = sr.ReceiverId,
            ReceiverName = sr.Receiver.FullName,
            ReceiverAvatarUrl = sr.Receiver.AvatarUrl,
            ReceiverRating = sr.Receiver.AverageRating,
            OfferedSkillId = sr.OfferedSkillId,
            OfferedSkillName = sr.OfferedSkill.Name,
            RequestedSkillId = sr.RequestedSkillId,
            RequestedSkillName = sr.RequestedSkill.Name,
            ProposedDate = sr.ProposedDate,
            DurationMinutes = sr.DurationMinutes,
            Status = sr.Status,
            Notes = sr.Notes,
            RejectionReason = sr.RejectionReason,
            CounterProposedDate = sr.CounterProposedDate,
            CounterDurationMinutes = sr.CounterDurationMinutes,
            CounterNotes = sr.CounterNotes,
            CreatedAt = sr.CreatedAt
        }).ToListAsync(cancellationToken);

        return Result<ICollection<SwapRequestDTO>>.Success(list);
    }
}
