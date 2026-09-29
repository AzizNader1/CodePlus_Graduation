using FluentValidation;

namespace SkillSwap.Application.Features.Sessions;

public class JoinSessionCallCommandValidator : AbstractValidator<JoinSessionCallCommand>
{
    public JoinSessionCallCommandValidator()
    {
        RuleFor(v => v.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");
    }
}
