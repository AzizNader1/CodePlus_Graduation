using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Payments;

public class CreateSubscriptionCheckoutCommandValidator : AbstractValidator<CreateSubscriptionCheckoutCommand>
{
    public CreateSubscriptionCheckoutCommandValidator()
    {
        RuleFor(x => x.Request.Plan).IsInEnum();
        RuleFor(x => x.Request.SuccessUrl).NotEmpty();
        RuleFor(x => x.Request.CancelUrl).NotEmpty();
    }
}
