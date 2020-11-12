using System.IO;
using System.Reflection;

namespace OpenStore.Infrastructure.Localization
{
    public class OpenStoreRequestLocalizationOptions
    {
        public OpenStoreRequestLocalizationSource Source { get; set; } = OpenStoreRequestLocalizationSource.Content;
        public string ContentSourcePattern { get; set; } = $"Resources{Path.DirectorySeparatorChar}localization_*.json";
        public string EmbeddedSourceRegexPattern { get; set; } = "localization_(.*)\\.(json)$";
        public Assembly EmbeddedResourceAssembly { get; set; } = Assembly.GetEntryAssembly();
        public char LocalizationResourceCultureSplitChar { get; set; } = '_';
    }
    
    public enum OpenStoreRequestLocalizationSource
    {
        Content, 
        EmbeddedSource
    }
}