using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace OpenStore.Infrastructure.Web.Theming
{
    public class ConfigurationThemeResolver : IThemeResolver
    {
        private readonly Theme _theme;

        public ConfigurationThemeResolver(IOptions<ThemeOptions> themeOptions)
        {
            // todo: support multi instance configuration
            _theme = themeOptions.Value != null ? new Theme(themeOptions.Value.Name) : Theme.Default;
        }

        public Task<ThemeContext> ResolveAsync(HttpContext context) => Task.FromResult(new ThemeContext(_theme));
    }
}