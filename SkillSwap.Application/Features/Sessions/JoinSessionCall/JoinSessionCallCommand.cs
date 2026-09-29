using MediatR;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;

namespace SkillSwap.Application.Features.Sessions;

public record JoinSessionCallCommand(Guid SessionId) : IRequest<Result<JoinSessionCallResponseDTO>>;
