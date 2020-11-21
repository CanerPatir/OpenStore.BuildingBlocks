using Microsoft.Extensions.Localization;

namespace OpenStore.Infrastructure.Localization
{
    public class NullLocalizer : IOpenStoreLocalizer
    {
        public LocalizedString this[string name] => GetLocalizedHtmlString(name);

        public LocalizedString this[string name, params object[] arguments] => GetLocalizedHtmlString(name, arguments);

        public LocalizedString GetLocalizedHtmlString(string key) => GetLocalizedHtmlString(key, null);

        public LocalizedString GetLocalizedHtmlString(string key, params object[] arguments) => new(key, key, true);
    }
}