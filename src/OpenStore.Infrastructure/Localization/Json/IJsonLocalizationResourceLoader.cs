using System.Globalization;

namespace OpenStore.Infrastructure.Localization.Json;

public interface IJsonLocalizationResourceLoader
{
    Dictionary<CultureInfo, Dictionary<string, string>> ReadResources(CultureInfo cultureInfo);
}