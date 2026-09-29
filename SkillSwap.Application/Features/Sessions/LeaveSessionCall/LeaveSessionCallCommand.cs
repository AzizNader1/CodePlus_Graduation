using MediatR;
using SkillSwap.Application.Common.Models;

namespace SkillSwap.Application.Features.Sessions;

public record LeaveSessionCallCommand(Guid SessionId) : IRequest<Result<bool>>;
