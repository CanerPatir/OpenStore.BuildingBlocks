using System;
using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenStore.Infrastructure.Localization.Json
{
    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IJsonLocalizationResourceLoader _localizationResourceLoader;
        private readonly ConcurrentDictionary<string, IStringLocalizer> _cache = new ConcurrentDictionary<string, IStringLocalizer>();
        private IOptions<OpenStoreJsonLocalizationOptions> _optionsOptions;

        public JsonStringLocalizerFactory(ILoggerFactory loggerFactory,
            IJsonLocalizationResourceLoader localizationResourceLoader,
            IOptions<OpenStoreJsonLocalizationOptions> optionsOptions)
        {
            _loggerFactory = loggerFactory;
            _localizationResourceLoader = localizationResourceLoader;
            _optionsOptions = optionsOptions;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            var cultureInfo = CultureInfo.CurrentUICulture;
            return GetCachedLocalizer(cultureInfo);
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            var cultureInfo = CultureInfo.CurrentUICulture;
            return GetCachedLocalizer(cultureInfo);
        }

        private IStringLocalizer GetCachedLocalizer(CultureInfo cultureInfo)
        {
            var cacheKey = cultureInfo.Name;
            return _cache.GetOrAdd(cacheKey,
                new JsonStringLocalizer(cultureInfo, _localizationResourceLoader, _optionsOptions, _loggerFactory.CreateLogger<JsonStringLocalizer>()));
        }

    }
}