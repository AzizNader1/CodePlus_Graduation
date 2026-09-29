using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.SwapRequests;

public class CreateSwapRequestCommandValidator : AbstractValidator<CreateSwapRequestCommand>
{
    public CreateSwapRequestCommandValidator()
    {
        RuleFor(x => x.Request.ReceiverId).NotEmpty();
        RuleFor(x => x.Request.OfferedSkillId).NotEmpty();
        RuleFor(x => x.Request.RequestedSkillId).NotEmpty();
        RuleFor(x => x.Request.DurationMinutes).GreaterThan(0);
    }
}
