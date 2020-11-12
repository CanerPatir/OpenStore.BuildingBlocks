using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenStore.Infrastructure.Localization
{
    public class EmbeddedLocalizationResourceLoader : ILocalizationResourceLoader
    {
        private readonly ILogger<EmbeddedLocalizationResourceLoader> _logger;
        private readonly OpenStoreRequestLocalizationOptions _options;
        private readonly Regex _regex;

        public EmbeddedLocalizationResourceLoader(IOptions<OpenStoreRequestLocalizationOptions> options, ILogger<EmbeddedLocalizationResourceLoader> logger)
        {
            _options = options.Value;
            
            _regex = new Regex(_options.EmbeddedSourceRegexPattern, RegexOptions.Compiled);
            _logger = logger;
        }

        public Dictionary<CultureInfo, Dictionary<string, string>> ReadResources(CultureInfo cultureInfo)
        {
            try
            {
                var manifestResourceNames = _options.EmbeddedResourceAssembly.GetManifestResourceNames();
                
                var localizationResources = manifestResourceNames.Where(x => _regex.IsMatch(x));
                var dictionaries = new Dictionary<CultureInfo, Dictionary<string, string>>();
                foreach (var resource in localizationResources)
                {
                    using var jsonStream = _options.EmbeddedResourceAssembly.GetManifestResourceStream(resource);
                    using var streamReader = new StreamReader(jsonStream ?? throw new ArgumentNullException(nameof(jsonStream)));

                    var culture = CultureInfo.GetCultureInfo(Path.GetFileNameWithoutExtension(resource).Split(_options.LocalizationResourceCultureSplitChar)[1]);
                    var dicValues = JsonSerializer.Deserialize<Dictionary<string, string>>(streamReader.ReadToEnd());
                    dictionaries.Add(culture, new Dictionary<string, string>(dicValues, StringComparer.OrdinalIgnoreCase));
                }

                return dictionaries;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Embedded localization resource could not load");
                throw;
            }
        }
    }
}