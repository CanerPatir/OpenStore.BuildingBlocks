using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenStore.Infrastructure.Localization.Json;
using OpenStore.Infrastructure.Localization.Resx;

namespace OpenStore.Infrastructure.Localization;

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
                opts.SetDefaultCulture(openStoreLocalizationOptions.DefaultUiCulture.Name);
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

        var openStoreResxLocalizationOptions = new OpenStoreResxLocalizationOptions()
        {
            Assembly = Assembly.GetCallingAssembly()
        };
        optionsBuilder(openStoreResxLocalizationOptions);
        if (openStoreResxLocalizationOptions.Assembly == null)
        {
            throw new ArgumentNullException(nameof(openStoreResxLocalizationOptions.Assembly));
        }

        mvcBuilder?
            .AddViewLocalization()
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create("SharedResource", openStoreResxLocalizationOptions.Assembly.FullName);
            });
        if (mvcBuilder != null)
        {
            services.Replace(ServiceDescriptor.Transient<IViewLocalizer>(sp =>
                new CustomViewLocalizer(sp.GetRequiredService<IHtmlLocalizerFactory>(), openStoreResxLocalizationOptions.Assembly.FullName)));
        }

        services.AddLocalization(opt => { opt.ResourcesPath = openStoreResxLocalizationOptions.ResourcesPath; });

        services
            .Configure(optionsBuilder)
            .Configure<RequestLocalizationOptions>(opts =>
            {
                opts.DefaultRequestCulture = new RequestCulture(openStoreResxLocalizationOptions.DefaultUiCulture, openStoreResxLocalizationOptions.DefaultUiCulture);
                opts.SupportedCultures = openStoreResxLocalizationOptions.DefaultSupportedUiCultures;
                opts.SupportedUICultures = openStoreResxLocalizationOptions.DefaultSupportedUiCultures;

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