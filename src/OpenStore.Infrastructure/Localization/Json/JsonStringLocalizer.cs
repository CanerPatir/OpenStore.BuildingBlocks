using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenStore.Infrastructure.Localization.Json;

public class JsonStringLocalizer : IStringLocalizer
{
    private Lazy<Dictionary<CultureInfo, Dictionary<string, string>>> _resources;
    private readonly CultureInfo _cultureInfo;
    private readonly IJsonLocalizationResourceLoader _resourceLoader;
    private readonly ILogger<JsonStringLocalizer> _logger;
    private readonly OpenStoreJsonLocalizationOptions _options;

    public JsonStringLocalizer(CultureInfo cultureInfo, IJsonLocalizationResourceLoader resourceLoader, 
        IOptions<OpenStoreJsonLocalizationOptions> optionsOptions,
        ILogger<JsonStringLocalizer> logger)
    {
        _resources = new Lazy<Dictionary<CultureInfo, Dictionary<string, string>>>(() => _resourceLoader.ReadResources(_cultureInfo));
        _cultureInfo = cultureInfo;
        _resourceLoader = resourceLoader;
        _logger = logger;
        _options = optionsOptions.Value;
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
            : _resources.Value[_options.DefaultUiCulture].TryGetValue(name, out value);
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => new LocalizedString[0];
}