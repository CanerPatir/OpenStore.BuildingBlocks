using System.IO;
using System.Reflection;

namespace OpenStore.Infrastructure.Localization.Json
{
    public class OpenStoreJsonLocalizationOptions : OpenStoreLocalizationOptions
    {
        public OpenStoreJsonLocalizationSource Source { get; set; } = OpenStoreJsonLocalizationSource.Content;
        public string ContentSourcePattern { get; set; } = $"Resources{Path.DirectorySeparatorChar}localization_*.json";
        public string EmbeddedSourceRegexPattern { get; set; } = "localization_(.*)\\.(json)$";
        public Assembly EmbeddedResourceAssembly { get; set; } = Assembly.GetEntryAssembly();
        public char LocalizationResourceCultureSplitChar { get; set; } = '_';
    }
}