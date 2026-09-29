using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Auth;

public class Verify2FaLoginCommandValidator : AbstractValidator<Verify2FaLoginCommand>
{
    public Verify2FaLoginCommandValidator()
    {
        RuleFor(x => x.Request.TwoFactorToken).NotEmpty();
        RuleFor(x => x.Request.Code).NotEmpty();
    }
}
