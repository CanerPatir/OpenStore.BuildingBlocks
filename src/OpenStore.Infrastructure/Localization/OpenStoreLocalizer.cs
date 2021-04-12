using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenStore.Infrastructure.Localization.Resx;

namespace OpenStore.Infrastructure.Localization
{
    public class OpenStoreLocalizer: IOpenStoreLocalizer
    {
        private readonly IStringLocalizer _localizer;

        public OpenStoreLocalizer(IStringLocalizerFactory localizerFactory, IOptions<OpenStoreResxLocalizationOptions> openStoreLocalizationOptionsOptions)
        {
            var openStoreLocalizationOptions = openStoreLocalizationOptionsOptions.Value;
            _localizer = localizerFactory.Create(openStoreLocalizationOptions.SharedResourceName, openStoreLocalizationOptions.SharedResourceAssemblyName);
        }
        
        public LocalizedString this[string name] => _localizer[name];
        
        public LocalizedString this[string name, params object[] arguments] => _localizer[name, arguments];
    }
}