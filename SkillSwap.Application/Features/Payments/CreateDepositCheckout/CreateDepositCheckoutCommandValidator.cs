using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Payments;

public class CreateDepositCheckoutCommandValidator : AbstractValidator<CreateDepositCheckoutCommand>
{
    public CreateDepositCheckoutCommandValidator()
    {
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.SuccessUrl).NotEmpty();
        RuleFor(x => x.Request.CancelUrl).NotEmpty();
    }
}
