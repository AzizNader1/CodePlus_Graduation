using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Auth;

public class Confirm2FaActivationCommandValidator : AbstractValidator<Confirm2FaActivationCommand>
{
    public Confirm2FaActivationCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().Length(6);
    }
}
