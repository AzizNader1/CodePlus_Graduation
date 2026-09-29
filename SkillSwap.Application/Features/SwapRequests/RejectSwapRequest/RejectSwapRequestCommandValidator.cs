using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.SwapRequests;

public class RejectSwapRequestCommandValidator : AbstractValidator<RejectSwapRequestCommand>
{
    public RejectSwapRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Request.Reason).NotEmpty().MaximumLength(500);
    }
}
