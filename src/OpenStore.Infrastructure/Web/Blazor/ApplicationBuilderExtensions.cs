using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenStore.Infrastructure.Web.Blazor
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseBlazorWasm(this IApplicationBuilder app, PathString path, string indexHtmlFile = "index.html")
        {
            var webHostEnvironment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments(path), first =>
            {
                if (webHostEnvironment.IsDevelopment())
                {
                    app.UseWebAssemblyDebugging();
                }

                first.UsePathBase(path);
                if (!webHostEnvironment.IsDevelopment())
                {
                    // ref: https://docs.telerik.com/blazor-ui/troubleshooting/deployment#reported-issues
                    StaticWebAssetsLoader.UseStaticWebAssets(webHostEnvironment, configuration);
                }

                first.UseBlazorFrameworkFiles();
                first.UseStaticFiles();
                first.UseRouting();

                first.UseEndpoints(endpoints => { endpoints.MapFallbackToFile(indexHtmlFile ?? "index.html"); });
            });

            return app;
        }
    }
}