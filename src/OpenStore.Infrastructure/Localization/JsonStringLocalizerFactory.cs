using System;
using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OpenStore.Infrastructure.Localization
{
    public class JsonStringLocalizerFactory : IReloadableStringLocalizerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILocalizationResourceLoader _localizationResourceLoader;
        private readonly ConcurrentDictionary<string, IReloadableJsonLocalizer> _cache = new ConcurrentDictionary<string, IReloadableJsonLocalizer>();

        public JsonStringLocalizerFactory(ILoggerFactory loggerFactory, ILocalizationResourceLoader localizationResourceLoader)
        {
            _loggerFactory = loggerFactory;
            _localizationResourceLoader = localizationResourceLoader;
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
            string cacheKey = cultureInfo.Name;
            return _cache.GetOrAdd(cacheKey, new JsonStringLocalizer(cultureInfo, _localizationResourceLoader, _loggerFactory.CreateLogger<JsonStringLocalizer>()));
        }

        public void Reload()
        {
            foreach (var item in _cache)
            {
                item.Value.ReloadResource();
            }
        }
    }
}