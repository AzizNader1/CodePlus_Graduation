using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.SwapRequests;

public class AcceptSwapRequestCommandValidator : AbstractValidator<AcceptSwapRequestCommand>
{
    public AcceptSwapRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
    }
}
