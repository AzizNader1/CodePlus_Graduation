using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Discovery;

public class GetRecommendedMatchesQueryValidator : AbstractValidator<GetRecommendedMatchesQuery>
{
    public GetRecommendedMatchesQueryValidator()
    {
    }
}
