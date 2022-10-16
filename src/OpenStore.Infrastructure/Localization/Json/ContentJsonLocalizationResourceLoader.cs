using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenStore.Infrastructure.Localization.Json;

public class ContentJsonLocalizationResourceLoader : IJsonLocalizationResourceLoader
{
    private readonly ILogger<ContentJsonLocalizationResourceLoader> _logger;
    private readonly OpenStoreJsonLocalizationOptions _options;

    public ContentJsonLocalizationResourceLoader(IOptions<OpenStoreJsonLocalizationOptions> options, ILogger<ContentJsonLocalizationResourceLoader> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Dictionary<CultureInfo, Dictionary<string, string>> ReadResources(CultureInfo cultureInfo)
    {
        try
        {
            var dictionaries = new Dictionary<CultureInfo, Dictionary<string, string>>();
            foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(_options.ContentSourcePattern), Path.GetFileName(_options.ContentSourcePattern),
                         SearchOption.TopDirectoryOnly))
            {
                var json = File.ReadAllText(file, Encoding.Default);

                var culture = CultureInfo.GetCultureInfo(Path.GetFileNameWithoutExtension(file).Split(_options.LocalizationResourceCultureSplitChar)[1]);
                var dicValues = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                dictionaries.Add(culture, new Dictionary<string, string>(dicValues, StringComparer.OrdinalIgnoreCase));
            }

            return dictionaries;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Content localization resource could not load");
            throw;
        }
    }
}