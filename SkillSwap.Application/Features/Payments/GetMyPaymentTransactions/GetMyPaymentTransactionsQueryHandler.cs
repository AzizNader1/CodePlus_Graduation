using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Payments;

public class GetMyPaymentTransactionsQueryHandler : IRequestHandler<GetMyPaymentTransactionsQuery, Result<ICollection<PaymentTransactionDTO>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyPaymentTransactionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ICollection<PaymentTransactionDTO>>> Handle(GetMyPaymentTransactionsQuery query, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<ICollection<PaymentTransactionDTO>>.Failure("Unauthorized");

        var txs = await _context.PaymentTransactions
            .Where(t => t.UserId == _currentUser.UserId.Value)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new PaymentTransactionDTO
            {
                Id = t.Id,
                Amount = t.Amount,
                Currency = t.Currency,
                Status = t.Status,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<ICollection<PaymentTransactionDTO>>.Success(txs);
    }
}
