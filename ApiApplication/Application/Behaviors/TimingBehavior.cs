using Lodgify.Api.Application.Extensions;
using System.Diagnostics;

namespace Lodgify.Api.Application.Behaviors;
public class TimingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TimingBehavior<TRequest, TResponse>> _logger;
    public TimingBehavior(ILogger<TimingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("----- Handling command {CommandName} ({@Command})", request.GetGenericTypeName(), request);
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();
        _logger.LogInformation("----- Command {CommandName} handled - response: {@Response} - Execution time {Elapsed}", request.GetGenericTypeName(), response, stopwatch.Elapsed);

        return response;
    }
}

