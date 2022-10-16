using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenStore.Infrastructure.Web.Swagger;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseOpenStoreSwaggerForModule(this IApplicationBuilder app, string name, string routePrefix = null)
    {
        if (!app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return app;
        }

        return app.UseSwagger()
            .UseSwaggerUI(c =>
            {
                if (!string.IsNullOrWhiteSpace(routePrefix) && !routePrefix.StartsWith("/"))
                {
                    routePrefix = "/" + routePrefix;
                }

                c.SwaggerEndpoint($"{routePrefix}/swagger/{name}/swagger.json", $"OpenStore {name} V1");
            });
    }
}