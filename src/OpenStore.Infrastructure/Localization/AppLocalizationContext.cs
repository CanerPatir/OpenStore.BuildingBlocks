using System.Globalization;

namespace OpenStore.Infrastructure.Localization
{
    public static class AppLocalizationContext
    {
        static AppLocalizationContext()
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

        public static CultureInfo[] DefaultSupportedUiCultures { get; }
        public static CultureInfo DefaultUiCulture { get; }
    }
}