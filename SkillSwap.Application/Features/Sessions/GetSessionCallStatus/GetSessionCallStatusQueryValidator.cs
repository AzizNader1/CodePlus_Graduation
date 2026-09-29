using FluentValidation;

namespace SkillSwap.Application.Features.Sessions;

public class GetSessionCallStatusQueryValidator : AbstractValidator<GetSessionCallStatusQuery>
{
    public GetSessionCallStatusQueryValidator()
    {
        RuleFor(v => v.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");
    }
}
