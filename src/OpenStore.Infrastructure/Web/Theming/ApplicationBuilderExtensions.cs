using Microsoft.AspNetCore.Builder;

namespace OpenStore.Infrastructure.Web.Theming;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseThemeSupport(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ThemeResolutionMiddleware>();
    }
}