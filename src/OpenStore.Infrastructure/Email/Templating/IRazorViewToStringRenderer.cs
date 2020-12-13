using System.Threading.Tasks;

namespace OpenStore.Infrastructure.Email.Templating
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string key, TModel model);
    }
}