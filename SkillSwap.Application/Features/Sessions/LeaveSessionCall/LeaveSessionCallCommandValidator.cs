using FluentValidation;

namespace SkillSwap.Application.Features.Sessions;

public class LeaveSessionCallCommandValidator : AbstractValidator<LeaveSessionCallCommand>
{
    public LeaveSessionCallCommandValidator()
    {
        RuleFor(v => v.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");
    }
}
