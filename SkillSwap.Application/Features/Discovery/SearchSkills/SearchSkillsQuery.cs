using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Discovery;

public record SearchSkillsQuery(
    string? Keyword,
    Guid? CategoryId,
    ProficiencyLevel? Level,
    double? MinRating,
    string? Location,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PaginatedList<UserSkillDTO>>>;
