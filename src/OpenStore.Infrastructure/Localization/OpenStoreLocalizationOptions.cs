using System.Globalization;

namespace OpenStore.Infrastructure.Localization;

public abstract class OpenStoreLocalizationOptions
{
    protected OpenStoreLocalizationOptions()
    {
        DefaultSupportedUiCultures = new[]
        {
            new CultureInfo("tr-TR"),
            new CultureInfo("en-US"),
            new CultureInfo("de-DE"),
            new CultureInfo("fr-FR"),
        };

        DefaultUiCulture = new CultureInfo("en-US");
    }

    public CultureInfo[] DefaultSupportedUiCultures { get; set; }

    public CultureInfo DefaultUiCulture { get; set; }
}