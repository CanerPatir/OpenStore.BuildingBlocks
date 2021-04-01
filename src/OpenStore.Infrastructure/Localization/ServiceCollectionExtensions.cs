using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenStore.Infrastructure.Localization.Json;
using OpenStore.Infrastructure.Localization.Resx;

namespace OpenStore.Infrastructure.Localization
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenStoreJsonLocalization(this IServiceCollection services) => AddOpenStoreJsonLocalization(services, null);

        public static IServiceCollection AddOpenStoreJsonLocalization(this IServiceCollection services, IMvcBuilder mvcBuilder) =>
            AddOpenStoreJsonLocalization(services, mvcBuilder, _ => { });

        public static IServiceCollection AddOpenStoreJsonLocalization(this IServiceCollection services, IMvcBuilder mvcBuilder,
            Action<OpenStoreJsonLocalizationOptions> optionsBuilder)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));

            var openStoreLocalizationOptions = new OpenStoreJsonLocalizationOptions();
            optionsBuilder(openStoreLocalizationOptions);

            mvcBuilder?.AddViewLocalization().AddDataAnnotationsLocalization();
            services
                .Configure(optionsBuilder)
                .Configure<RequestLocalizationOptions>(opts =>
                {
                    opts.DefaultRequestCulture = new RequestCulture(openStoreLocalizationOptions.DefaultUiCulture, openStoreLocalizationOptions.DefaultUiCulture);
                    opts.SupportedCultures = openStoreLocalizationOptions.DefaultSupportedUiCultures;
                    opts.SupportedUICultures = openStoreLocalizationOptions.DefaultSupportedUiCultures;
                })
                .AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>()
                .AddSingleton(sp =>
                    sp.GetRequiredService<IStringLocalizerFactory>().Create("SharedResource", openStoreLocalizationOptions.EmbeddedResourceAssembly.FullName))
                .AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>))
                .AddSingleton<IJsonLocalizationResourceLoader>(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<OpenStoreJsonLocalizationOptions>>();
                    var source = options.Value.Source;

                    return source switch
                    {
                        OpenStoreJsonLocalizationSource.Content => new ContentJsonLocalizationResourceLoader(options,
                            sp.GetRequiredService<ILogger<ContentJsonLocalizationResourceLoader>>()),
                        OpenStoreJsonLocalizationSource.EmbeddedSource => new EmbeddedJsonLocalizationResourceLoader(options,
                            sp.GetRequiredService<ILogger<EmbeddedJsonLocalizationResourceLoader>>()),
                        _ => throw new Exception()
                    };
                })
                .AddSingleton<IOpenStoreLocalizer, OpenStoreLocalizer>()
                ;

            return services;
        }

        // -- resx

        public static IServiceCollection AddOpenStoreResxLocalization(this IServiceCollection services) => AddOpenStoreResxLocalization(services, null);

        public static IServiceCollection AddOpenStoreResxLocalization(this IServiceCollection services, IMvcBuilder mvcBuilder) =>
            AddOpenStoreResxLocalization(services, mvcBuilder, _ => { });

        public static IServiceCollection AddOpenStoreResxLocalization(this IServiceCollection services, IMvcBuilder mvcBuilder,
            Action<OpenStoreResxLocalizationOptions> optionsBuilder)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));

            var openStoreLocalizationOptions = new OpenStoreResxLocalizationOptions();
            optionsBuilder(openStoreLocalizationOptions);

            mvcBuilder?.AddViewLocalization().AddDataAnnotationsLocalization();

            services.AddLocalization(opt =>
            {
                opt.ResourcesPath = openStoreLocalizationOptions.ResourcesPath;
            });
            services.AddSingleton(sp =>
                sp.GetRequiredService<IStringLocalizerFactory>().Create(openStoreLocalizationOptions.SharedResourceName, openStoreLocalizationOptions.SharedResourceAssemblyName));

            services
                .Configure(optionsBuilder)
                .Configure<RequestLocalizationOptions>(opts =>
                {
                    opts.DefaultRequestCulture = new RequestCulture(openStoreLocalizationOptions.DefaultUiCulture, openStoreLocalizationOptions.DefaultUiCulture);
                    opts.SupportedCultures = openStoreLocalizationOptions.DefaultSupportedUiCultures;
                    opts.SupportedUICultures = openStoreLocalizationOptions.DefaultSupportedUiCultures;
                    var provider = opts.RequestCultureProviders.SingleOrDefault(x => x is CookieRequestCultureProvider);
                    if (provider is CookieRequestCultureProvider cookieRequestCultureProvider)
                    {
                        cookieRequestCultureProvider.CookieName = LocalizationConstants.DefaultCookieName;
                    }
                })
                .AddSingleton<IOpenStoreLocalizer, OpenStoreLocalizer>()
                ;

            return services;
        }
    }
}