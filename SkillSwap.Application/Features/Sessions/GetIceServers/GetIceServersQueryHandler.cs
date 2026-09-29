using FluentValidation;
using MediatR;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;

namespace SkillSwap.Application.Features.Sessions;

public class GetIceServersQueryHandler : IRequestHandler<GetIceServersQuery, Result<IReadOnlyList<IceServerConfigDTO>>>
{
    private readonly IVideoCallService _videoCallService;
    private readonly ICurrentUserService _currentUser;

    public GetIceServersQueryHandler(
        IVideoCallService videoCallService,
        ICurrentUserService currentUser)
    {
        _videoCallService = videoCallService;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<IceServerConfigDTO>>> Handle(GetIceServersQuery query, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == null)
            return Result<IReadOnlyList<IceServerConfigDTO>>.Failure("Unauthorized");

        var iceServers = _videoCallService.GetIceServers();
        return await Task.FromResult(Result<IReadOnlyList<IceServerConfigDTO>>.Success(iceServers));
    }
}
