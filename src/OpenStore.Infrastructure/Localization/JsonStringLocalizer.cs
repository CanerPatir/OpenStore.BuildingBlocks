using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OpenStore.Infrastructure.Localization
{
    public class JsonStringLocalizer : IReloadableJsonLocalizer
    {
        private Lazy<Dictionary<CultureInfo, Dictionary<string, string>>> _resources;
        private readonly CultureInfo _cultureInfo;
        private readonly ILocalizationResourceLoader _resourceLoader;
        private readonly ILogger<JsonStringLocalizer> _logger;

        public JsonStringLocalizer(CultureInfo cultureInfo, ILocalizationResourceLoader resourceLoader, ILogger<JsonStringLocalizer> logger)
        {
            ReloadResource();
            _cultureInfo = cultureInfo;
            _resourceLoader = resourceLoader;
            _logger = logger;
        }

        public void ReloadResource()
        {
            _resources = new Lazy<Dictionary<CultureInfo, Dictionary<string, string>>>(() => _resourceLoader.ReadResources(_cultureInfo));
        }

        public LocalizedString this[string name] => this[name, null];

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null) throw new ArgumentNullException(nameof(name));
                if (TryGetResource(name, out var value))
                {
                    return new LocalizedString(name, arguments == null ? value : string.Format(value, arguments), resourceNotFound: false);
                }

                return new LocalizedString(name, arguments == null ? name : string.Format(name, arguments), resourceNotFound: true);
            }
        }

        private bool TryGetResource(string name, out string value)
        {
            return _resources.Value.ContainsKey(CultureInfo.CurrentCulture)
                ? _resources.Value[CultureInfo.CurrentCulture].TryGetValue(name, out value)
                : _resources.Value[AppLocalizationContext.DefaultUiCulture].TryGetValue(name, out value);
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => new LocalizedString[0];
    }
}