using Microsoft.AspNetCore.Http;

namespace OpenStore.Infrastructure.Web.Theming;

public interface IThemeResolver
{
    Task<ThemeContext> ResolveAsync(HttpContext context);
}