using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenStore.Infrastructure.Web.Theming
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddThemeSupport(this IServiceCollection services, IConfiguration configuration) => services.AddThemeSupport<ConfigurationThemeResolver>(configuration);

        public static IServiceCollection AddThemeSupport<TResolver>(this IServiceCollection services, IConfiguration configuration)
            where TResolver : class, IThemeResolver
        {
            services.Configure<RazorViewEngineOptions>(options => { options.ViewLocationExpanders.Add(new ThemePageViewLocationExpander()); });

            services.Configure<RazorPagesOptions>(options => { options.RootDirectory = "/Themes/Default"; });

            services.Configure<ThemeOptions>(configuration.GetSection("Theme"));
            services.AddScoped<IThemeResolver, TResolver>();
            services.AddScoped(sp => sp.GetService<IHttpContextAccessor>()?.HttpContext?.GetThemeContext());
            services.AddScoped(sp => sp.GetService<ThemeContext>()?.Theme);

            return services;
        }
    }
}