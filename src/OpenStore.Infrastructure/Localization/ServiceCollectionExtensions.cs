using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenStore.Infrastructure.Localization
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenStoreLocalization(this IServiceCollection services) => AddOpenStoreLocalization(services, _ => { });

        public static IServiceCollection AddOpenStoreLocalization(this IServiceCollection services, Action<OpenStoreRequestLocalizationOptions> optionsBuilder)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));
            
            services
                .Configure<RequestLocalizationOptions>(opts =>
                {
                    opts.DefaultRequestCulture = new RequestCulture(AppLocalizationContext.DefaultUiCulture, AppLocalizationContext.DefaultUiCulture);
                    opts.SupportedCultures = AppLocalizationContext.DefaultSupportedUiCultures;
                    opts.SupportedUICultures = AppLocalizationContext.DefaultSupportedUiCultures;
                })
                .Configure(optionsBuilder)
                .AddSingleton<IReloadableStringLocalizerFactory, JsonStringLocalizerFactory>()
                .AddSingleton<IStringLocalizerFactory>(c => c.GetRequiredService<IReloadableStringLocalizerFactory>())
                .AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>))
                .AddSingleton<ILocalizationResourceLoader>(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<OpenStoreRequestLocalizationOptions>>();
                    var source = options.Value.Source;

                    return source switch
                    {
                        OpenStoreRequestLocalizationSource.Content => new ContentLocalizationResourceLoader(options, sp.GetRequiredService<ILogger<ContentLocalizationResourceLoader>>()),
                        OpenStoreRequestLocalizationSource.EmbeddedSource => new EmbeddedLocalizationResourceLoader(options, sp.GetRequiredService<ILogger<EmbeddedLocalizationResourceLoader>>()),
                        _ => throw new Exception()
                    };
                })
                .AddSingleton<IOpenStoreLocalizer, OpenStoreLocalizer>()
                // .AddHostedService<LocalizationResourceChangeWatcher>()
                ;

            return services;
        }
    }
}