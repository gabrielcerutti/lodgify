using Showtime.Api.Application.Extensions;
using System.Diagnostics;

namespace Showtime.Api.Application.Behaviors;
public class TrackingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TrackingBehavior<TRequest, TResponse>> _logger;
    public TrackingBehavior(ILogger<TrackingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("----- Handling command {CommandName} ({@Command})", request.GetGenericTypeName(), request);
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();
        _logger.LogInformation("----- Command {CommandName} handled - Execution time {Elapsed} ms - response: {@Response}", request.GetGenericTypeName(), stopwatch.Elapsed.TotalMilliseconds, response);        

        return response;
    }
}

