using MediatR;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;

namespace SkillSwap.Application.Features.Sessions;

public record SendSessionHeartbeatCommand(Guid SessionId, SessionHeartbeatRequest? Request = null) : IRequest<Result<bool>>;
