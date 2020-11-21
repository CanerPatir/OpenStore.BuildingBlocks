using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenStore.Application.Exceptions;
using OpenStore.Domain;
using OpenStore.Infrastructure.Localization;

namespace OpenStore.Infrastructure.Web.ErrorHandling
{
    public class ApplicationErrorMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next, PathString errorHandlingPath)
        {
            Exception ex = null;
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                ex = e;
            }

            if (ex == null) return;

            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("UseOpenStoreApiExceptionHandling");
            logger.LogError(ex.Demystify(), "OpenStore http request error");

            if (!context.IsApiController())
            {
                context.Response.Redirect(errorHandlingPath);
                return;
            }

            context.Response.StatusCode = ex switch
            {
                ResourceNotFoundException resourceNotFoundException => 404,
                DomainException domainException => 400,
                _ => 500
            };

            if (context.Response.StatusCode == 404)
            {
                await context.Response.WriteAsync("Not Found");
            }
            else
            {
                var loc = context.RequestServices.GetService<IOpenStoreLocalizer>();
                if (loc != null)
                {
                    await context.Response.WriteAsync(loc[ex.Message]);
                }
                else
                {
                    await context.Response.WriteAsync(ex.Message);
                }
            }
        }
    }
}