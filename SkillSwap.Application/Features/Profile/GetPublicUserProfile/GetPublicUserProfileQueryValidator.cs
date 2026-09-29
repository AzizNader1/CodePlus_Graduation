using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Features.Profile;

public class GetPublicUserProfileQueryValidator : AbstractValidator<GetPublicUserProfileQuery>
{
    public GetPublicUserProfileQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
