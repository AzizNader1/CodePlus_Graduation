using MediatR;
using Microsoft.Extensions.Logging;

namespace SkillSwap.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior logging execution of application requests.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    public LoggingBehavior(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Processing SkillSwap Request: {Name}", requestName);

        var response = await next();

        _logger.LogInformation("Completed SkillSwap Request: {Name}", requestName);
        return response;
    }
}
