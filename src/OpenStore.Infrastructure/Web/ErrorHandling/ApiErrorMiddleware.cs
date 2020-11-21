using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenStore.Application.Exceptions;
using OpenStore.Domain;
using OpenStore.Infrastructure.Localization;

namespace OpenStore.Infrastructure.Web.ErrorHandling
{
    public class ApiErrorMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiErrorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Exception ex = null;
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                ex = e;
            }

            if (ex == null) return;

            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("UseOpenStoreApiExceptionHandling");
            logger.LogError(ex.Demystify(), "OpenStore http request error");

            context.Response.StatusCode = ex switch
            {
                ResourceNotFoundException => 404,
                DomainException => 400,
                ValidationException => 400,
                _ => 500
            };

            var loc = context.RequestServices.GetService<IOpenStoreLocalizer>() ?? new NullLocalizer();

            var errorDto = ex switch
            {
                ResourceNotFoundException resourceNotFoundException => new OpenStoreWebErrorDto(loc[resourceNotFoundException.Message], ArraySegment<string>.Empty),
                DomainException domainException => new OpenStoreWebErrorDto(loc[domainException.Message], ArraySegment<string>.Empty),
                ValidationException validationException => new OpenStoreWebErrorDto(loc[validationException.Message], validationException.Errors.Select(x => loc[x.Message].ToString())),
                _ => new OpenStoreWebErrorDto("Generic error", ArraySegment<string>.Empty)
            };
            
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorDto));
        }
    }
}