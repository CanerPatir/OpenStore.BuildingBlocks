using Microsoft.AspNetCore.Http;

namespace OpenStore.Infrastructure.Web.Theming;

public static class HttpContextExtensions
{
    private const string ThemeContextKey = "ThemeContext";

    public static void SetThemeContext(this HttpContext context, ThemeContext themeContext)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (themeContext == null)
            throw new ArgumentNullException(nameof(themeContext));

        context.Items[ThemeContextKey] = themeContext;
    }

    public static ThemeContext GetThemeContext(this HttpContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (context.Items.TryGetValue(ThemeContextKey, out var themeContext))
        {
            return themeContext as ThemeContext;
        }

        return null;
    }

    public static Theme GetTheme(this HttpContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var themeContext = GetThemeContext(context);

        return themeContext?.Theme;
    }
}