using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Skills;

public class AddOfferedSkillCommandValidator : AbstractValidator<AddOfferedSkillCommand>
{
    public AddOfferedSkillCommandValidator()
    {
        RuleFor(x => x.Request.SkillId).NotEmpty();
        RuleFor(x => x.Request.ProficiencyLevel).IsInEnum();
    }
}
