using Microsoft.Extensions.Localization;

namespace OpenStore.Infrastructure.Localization
{
    public interface IReloadableStringLocalizerFactory : IStringLocalizerFactory
    {
        void Reload();
    }
}