using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenStore.Infrastructure.Web.Spa;

public enum SpaType
{
    Angular,
    React
}
// To learn more about options for serving an Angular SPA from ASP.NET Core,
// see https://go.microsoft.com/fwlink/?linkid=864501
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseOpenStoreSpa(this IApplicationBuilder app, string sourcePath, SpaType spaType, string developmentServerProxyUri = null, string npmScript = "start")
    {
        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            
        app.UseSpaStaticFiles();
        if (env.IsDevelopment())
        {
            if (!string.IsNullOrWhiteSpace(developmentServerProxyUri))
            {
                app.MapWhen(WebPackDevServerMatcher, webpackDevServer => { webpackDevServer.UseSpa(spa => { spa.UseProxyToSpaDevelopmentServer(developmentServerProxyUri); }); });
            }
        }

        app.UseSpa(spa =>
        {
            spa.Options.SourcePath = sourcePath;
            if (!env.IsDevelopment())
                return;

            switch (spaType)
            {
                case SpaType.Angular:
                    spa.UseAngularCliServer(npmScript: npmScript);
                    break;
                case SpaType.React:
                    spa.UseReactDevelopmentServer(npmScript: npmScript);
                    break;
                default:
                    throw new NotSupportedException("Not supported spa type");
            }

            if (!string.IsNullOrWhiteSpace(developmentServerProxyUri))
            {
                spa.UseProxyToSpaDevelopmentServer(developmentServerProxyUri);
            }
        });

        return app;
    }

    public static IApplicationBuilder UseOpenStoreSpaForPath(this IApplicationBuilder app, string sourcePath, SpaType spaType, PathString pathString, string developmentServerProxyBaseUri = "https://localhost:5001", string npmScript = "start")
    {
        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        if (env.IsDevelopment())
        {
            app.MapWhen(WebPackDevServerMatcher, webpackDevServer => { webpackDevServer.UseSpa(spa => { spa.UseProxyToSpaDevelopmentServer(CombineProxyUri(developmentServerProxyBaseUri, pathString)); }); });
        }

        app.Map(pathString, adminApp =>
        {
            adminApp.UseSpaStaticFiles();
            adminApp.UseSpa(spa =>
            {
                spa.Options.SourcePath = sourcePath;

                if (!env.IsDevelopment())
                    return;

                switch (spaType)
                {
                    case SpaType.Angular:
                        spa.UseAngularCliServer(npmScript: npmScript);
                        break;
                    case SpaType.React:
                        spa.UseReactDevelopmentServer(npmScript: npmScript);
                        break;
                    default:
                        throw new NotSupportedException("Not supported spa type");
                }

                spa.UseProxyToSpaDevelopmentServer(CombineProxyUri(developmentServerProxyBaseUri, pathString));
            });
        });

        return app;
    }

    private static string CombineProxyUri(string proxyBaseUri, PathString pathString)
    {
        if (proxyBaseUri.EndsWith("/"))
            proxyBaseUri = proxyBaseUri.Remove(proxyBaseUri.Length - 1);
            
        return proxyBaseUri + pathString;
    }

    // Captures the requests generated when using webpack dev server in the following ways:
    // via: https://localhost:5001/app/ (inline mode)
    // via: https://localhost:5001/webpack-dev-server/app/  (iframe mode)
    // captures requests like these:
    // https://localhost:5001/webpack_dev_server.js
    // https://localhost:5001/webpack_dev_server/app/
    // https://localhost:5001/__webpack_dev_server__/live.bundle.js
    // wss://localhost:5001/sockjs-node/978/qhjp11ck/websocket
    private static bool WebPackDevServerMatcher(HttpContext context)
    {
        var pathString = context.Request.Path.ToString();
        return pathString.Contains(context.Request.PathBase.Add("/webpack-dev-server")) ||
               context.Request.Path.StartsWithSegments("/__webpack_dev_server__") ||
               context.Request.Path.StartsWithSegments("/sockjs-node");
    }
}