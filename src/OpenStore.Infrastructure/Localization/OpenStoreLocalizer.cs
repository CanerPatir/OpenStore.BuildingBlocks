using Microsoft.Extensions.Localization;

namespace OpenStore.Infrastructure.Localization
{
    public class OpenStoreLocalizer : IOpenStoreLocalizer
    {
        private readonly IStringLocalizer _localizer;

        public OpenStoreLocalizer(IStringLocalizerFactory factory)
        {
            _localizer = factory.Create("SharedResource", "");
        }

        public LocalizedString this[string name] => GetLocalizedHtmlString(name);
        
        public LocalizedString this[string name, params object[] arguments] => GetLocalizedHtmlString(name, arguments);
        
        public LocalizedString GetLocalizedHtmlString(string key)
        {
            return _localizer[key];
        }
        
        public LocalizedString GetLocalizedHtmlString(string key, params object[] argumants)
        {
            return _localizer[key, argumants];
        }
    }
}