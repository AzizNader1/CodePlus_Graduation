using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Reviews;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.Request.SessionId).NotEmpty();
        RuleFor(x => x.Request.OverallRating).InclusiveBetween(1, 5);
        RuleFor(x => x.Request.PunctualityScore).InclusiveBetween(1, 5);
        RuleFor(x => x.Request.CommunicationScore).InclusiveBetween(1, 5);
        RuleFor(x => x.Request.KnowledgeScore).InclusiveBetween(1, 5);
    }
}
