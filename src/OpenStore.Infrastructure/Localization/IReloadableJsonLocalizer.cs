using Microsoft.Extensions.Localization;

namespace OpenStore.Infrastructure.Localization
{
    public interface IReloadableJsonLocalizer : IStringLocalizer
    {
        void ReloadResource();
    }
}