using MediatR;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;

namespace SkillSwap.Application.Features.Auth;

public record FacebookLoginCommand(FacebookLoginRequest Request) : IRequest<Result<AuthResponseDTO>>;
