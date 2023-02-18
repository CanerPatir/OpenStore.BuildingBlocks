using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenStore.Application.Exceptions;

namespace OpenStore.Infrastructure.CommandBus;

public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILoggerFactory _loggerFactory;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest);
        var logger = _loggerFactory.CreateLogger(requestType);
        var scopeVariables = new Dictionary<string, object>
        {
            { OpenStoreConstants.CorrelationIdKey, Guid.NewGuid() },
            { "IRequest", requestType.Name }
        };
        var scope = logger.BeginScope(scopeVariables);
        logger.LogDebug("Handling");
        try
        {
            var response = await next();
            logger.LogDebug("Handled successfully");
            return response;
        }
        catch (Exception e)
        {
            if (e is not ResourceNotFoundException)
            {
                logger.LogError(e.Demystify(), "Handling error");
            }

            throw;
        }
        finally
        {
            scope.Dispose();
        }
    }
}