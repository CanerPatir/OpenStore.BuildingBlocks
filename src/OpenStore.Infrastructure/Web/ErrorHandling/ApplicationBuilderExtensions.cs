using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenStore.Application.Exceptions;
using OpenStore.Domain;
using OpenStore.Infrastructure.Localization;

namespace OpenStore.Infrastructure.Web.ErrorHandling
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseOpenStoreErrorHandling(this IApplicationBuilder appBuilder, PathString errorHandlingPath)
        {
            var env = appBuilder.ApplicationServices.GetRequiredService<IHostEnvironment>();
            appBuilder.UseStatusCodePagesWithReExecute("/error?statusCode={0}");
            appBuilder.UseWhen(context => !context.IsApiController() && env.IsDevelopment(),
                app => app
                    .UseDeveloperExceptionPage()
                    .UseDatabaseErrorPage());

            return appBuilder.UseWhen(context => context.IsApiController() || !env.IsDevelopment(),
                app => app.Use(async (context, next) =>
                {
                    Exception ex = null;
                    try
                    {
                        await next();
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
                }));
        }

        private static bool IsApiController(this HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            return endpoint != null && endpoint.Metadata.Any(x => x.GetType() == typeof(ApiControllerAttribute));
        }
    }
}