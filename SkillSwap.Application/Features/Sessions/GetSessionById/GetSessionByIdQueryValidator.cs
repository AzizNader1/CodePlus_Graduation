using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Sessions;

public class GetSessionByIdQueryValidator : AbstractValidator<GetSessionByIdQuery>
{
    public GetSessionByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
