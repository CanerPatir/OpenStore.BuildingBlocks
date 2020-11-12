using System.Collections.Generic;
using System.Globalization;

namespace OpenStore.Infrastructure.Localization
{
    public interface ILocalizationResourceLoader
    {
        Dictionary<CultureInfo, Dictionary<string, string>> ReadResources(CultureInfo cultureInfo);
    }
}