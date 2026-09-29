using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Application.Features.Auth;

namespace SkillSwap.Api.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await Mediator.Send(new RegisterCommand(request));
        return HandleResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await Mediator.Send(new LoginCommand(request));
        return HandleResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await Mediator.Send(new RefreshTokenCommand(request));
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult> Enable2Fa()
    {
        var result = await Mediator.Send(new Enable2FaCommand());
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult> Confirm2Fa([FromBody] Confirm2FaRequest request)
    {
        var result = await Mediator.Send(new Confirm2FaActivationCommand(request.Code));
        return HandleResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> Verify2Fa([FromBody] Verify2FaRequest request)
    {
        var result = await Mediator.Send(new Verify2FaLoginCommand(request));
        return HandleResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var result = await Mediator.Send(new GoogleLoginCommand(request));
        return HandleResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> AppleLogin([FromBody] AppleLoginRequest request)
    {
        var result = await Mediator.Send(new AppleLoginCommand(request));
        return HandleResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> FacebookLogin([FromBody] FacebookLoginRequest request)
    {
        var result = await Mediator.Send(new FacebookLoginCommand(request));
        return HandleResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await Mediator.Send(new ForgotPasswordCommand(request));
        return HandleResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await Mediator.Send(new ResetPasswordCommand(request));
        return HandleResult(result);
    }
}
