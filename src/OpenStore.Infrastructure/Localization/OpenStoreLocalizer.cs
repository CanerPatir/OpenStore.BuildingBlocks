using Microsoft.Extensions.Localization;

namespace OpenStore.Infrastructure.Localization
{
    public class OpenStoreLocalizer: IOpenStoreLocalizer
    {
        private readonly IStringLocalizer _localizer;

        public OpenStoreLocalizer(IStringLocalizer localizer)
        {
            _localizer = localizer;
        }

        public LocalizedString this[string name] => GetLocalizedHtmlString(name);
        
        public LocalizedString this[string name, params object[] arguments] => GetLocalizedHtmlString(name, arguments);
        
        public LocalizedString GetLocalizedHtmlString(string key) => _localizer[key];

        public LocalizedString GetLocalizedHtmlString(string key, params object[] arguments) => _localizer[key, arguments];
    }
}