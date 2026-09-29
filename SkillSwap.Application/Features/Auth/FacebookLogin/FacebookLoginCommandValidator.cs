using FluentValidation;

namespace SkillSwap.Application.Features.Auth;

public class FacebookLoginCommandValidator : AbstractValidator<FacebookLoginCommand>
{
    public FacebookLoginCommandValidator()
    {
        RuleFor(v => v.Request.AccessToken)
            .NotEmpty().WithMessage("Facebook Access Token is required.");
    }
}
