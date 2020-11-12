using Microsoft.Extensions.Localization;

namespace OpenStore.Infrastructure.Localization
{
    public interface IOpenStoreLocalizer
    {
        LocalizedString this[string name] { get; }
        LocalizedString this[string name, params object[] arguments] { get; }
        LocalizedString GetLocalizedHtmlString(string key);
        LocalizedString GetLocalizedHtmlString(string key, params object[] argumants);
    }
}