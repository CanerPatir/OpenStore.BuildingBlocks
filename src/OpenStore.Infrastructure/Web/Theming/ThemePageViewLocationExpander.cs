using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace OpenStore.Infrastructure.Web.Theming
{
    public class ThemePageViewLocationExpander : IViewLocationExpander
    {
        private const string ThemeKey = "theme";

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.Values.TryGetValue(ThemeKey, out var theme))
            {
                viewLocations = new[] {
                    $"/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                    $"/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                }.Concat(viewLocations);
            }

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            context.Values[ThemeKey] = context.ActionContext.HttpContext.GetTheme()?.Name;
        }
    }

}