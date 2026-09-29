using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Skills;

public class GetSkillsQueryValidator : AbstractValidator<GetSkillsQuery>
{
    public GetSkillsQueryValidator()
    {
        When(x => !string.IsNullOrEmpty(x.Search), () => {
            RuleFor(x => x.Search).MaximumLength(100);
        });
    }
}
