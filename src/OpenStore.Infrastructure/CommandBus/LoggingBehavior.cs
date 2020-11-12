using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace OpenStore.Infrastructure.CommandBus
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
         private readonly ILoggerFactory _loggerFactory;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ILoggerFactory loggerFactory)
        {
             _loggerFactory = loggerFactory;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var logger = _loggerFactory.CreateLogger(typeof(TRequest));
            logger.LogInformation("Handling");
            try
            {
                var response = await next();
                logger.LogInformation("Handled successfully");
                return response;
            }
            catch (Exception e)
            {
                logger.LogError(e.Demystify(), "Handling error");
                throw;
            }
        }
    }
}