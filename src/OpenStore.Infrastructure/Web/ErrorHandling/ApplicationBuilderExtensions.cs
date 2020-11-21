using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenStore.Infrastructure.Web.ErrorHandling
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseOpenStoreErrorHandling(this IApplicationBuilder appBuilder, PathString errorHandlingPath)
        {
            var env = appBuilder.ApplicationServices.GetRequiredService<IHostEnvironment>();

            appBuilder.UseWhen(context => context.IsApiController(), app => app.UseMiddleware<ApplicationErrorMiddleware>(errorHandlingPath));
            return appBuilder.UseWhen(context => !context.IsApiController(), 
                app =>
                {
                    if (env.IsDevelopment())
                    {
                        app
                            .UseStatusCodePagesWithReExecute(errorHandlingPath, "?statusCode={0}")
                            .UseDeveloperExceptionPage().UseDatabaseErrorPage();
                    }
                    else
                    {
                        app.UseMiddleware<ApplicationErrorMiddleware>(errorHandlingPath);
                    }
                });
        }

        internal static bool IsApiController(this HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            return endpoint != null && endpoint.Metadata.Any(x => x.GetType() == typeof(ApiControllerAttribute));
        }
    }
}