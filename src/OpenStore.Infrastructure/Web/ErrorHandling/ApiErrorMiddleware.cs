using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenStore.Application.Exceptions;
using OpenStore.Domain;
using OpenStore.Infrastructure.Localization;

namespace OpenStore.Infrastructure.Web.ErrorHandling;

public class ApiErrorMiddleware
{
    private readonly RequestDelegate _next;

    public ApiErrorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<ApiErrorMiddleware> logger, IOpenStoreLocalizer loc)
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

        switch (ex)
        {
            case ResourceNotFoundException:
                context.Response.StatusCode = 404;
                break;
            case ValidationException:
            case ApplicationException:
            case DomainException:
                logger.LogError(ex.Message, $"OpenStore {ex.GetType().Name}");
                context.Response.StatusCode = 400;
                break;
            default:
                logger.LogError(ex.Demystify(), "OpenStore unknown exception");
                context.Response.StatusCode = 500;
                break;
        }

        loc ??= new NullLocalizer();

        var errorDto = ex switch
        {
            ValidationException validationException => new OpenStoreWebErrorDto(loc[validationException.Message],
                validationException.Errors.Select(x => loc[x.Message].ToString())),
            ResourceNotFoundException resourceNotFoundException => new OpenStoreWebErrorDto(loc[resourceNotFoundException.Message], ArraySegment<string>.Empty),
            ApplicationException applicationException => new OpenStoreWebErrorDto(loc[applicationException.Message], ArraySegment<string>.Empty),
            DomainException domainException => new OpenStoreWebErrorDto(loc[domainException.Message], ArraySegment<string>.Empty),

            _ => new OpenStoreWebErrorDto(GenericErrorKey, new[] { GenericErrorKey }),
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorDto));
    }

    private const string GenericErrorKey = "OpenStore.GenericError";
}