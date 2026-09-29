using FluentValidation;

namespace SkillSwap.Application.Features.Sessions;

public class SendSessionHeartbeatCommandValidator : AbstractValidator<SendSessionHeartbeatCommand>
{
    public SendSessionHeartbeatCommandValidator()
    {
        RuleFor(v => v.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");
    }
}
