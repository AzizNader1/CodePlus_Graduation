using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Reports;

public class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportCommandValidator()
    {
        RuleFor(x => x.Request.ReportedUserId).NotEmpty();
        RuleFor(x => x.Request.ReasonCategory).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Details).NotEmpty().MaximumLength(2000);
    }
}
