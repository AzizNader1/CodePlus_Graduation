using FluentValidation;

namespace SkillSwap.Application.Features.Sessions;

public class GetIceServersQueryValidator : AbstractValidator<GetIceServersQuery>
{
    public GetIceServersQueryValidator()
    {
        RuleFor(v => v.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");
    }
}
