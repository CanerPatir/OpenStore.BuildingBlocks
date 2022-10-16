using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenStore.Infrastructure.Web.ErrorHandling;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Error handler for MVC apps
    /// </summary>
    /// <param name="app"></param>
    /// <param name="errorHandlingPath"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseOpenStoreMvcErrorHandling(this IApplicationBuilder app, PathString errorHandlingPath)
    {
        var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
        app.UseStatusCodePagesWithReExecute(errorHandlingPath, "?statusCode={0}");

        // if (env.IsDevelopment())
        // {
        //     return app.UseDeveloperExceptionPage().UseDatabaseErrorPage();
        // }

        return app.UseMiddleware<MvcErrorMiddleware>(errorHandlingPath);
    }

    /// <summary>
    /// Error handler for web api apps. Please configure your api behavior in the Startup.ConfigureServices as below.
    /// services.AddControllers()
    ///      .ConfigureApiBehaviorOptions(options =>
    ///      {
    ///          options.SuppressModelStateInvalidFilter = true;
    ///      });
    /// </summary>
    /// <param name="appBuilder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseOpenStoreApiErrorHandling(this IApplicationBuilder appBuilder)
    {
        return appBuilder.UseMiddleware<ApiErrorMiddleware>();
    }
}