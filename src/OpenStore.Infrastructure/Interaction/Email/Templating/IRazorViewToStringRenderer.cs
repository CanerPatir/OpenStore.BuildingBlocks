using System.Threading.Tasks;

namespace OpenStore.Infrastructure.Interaction.Email.Templating
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewPath, TModel model);
    }
}