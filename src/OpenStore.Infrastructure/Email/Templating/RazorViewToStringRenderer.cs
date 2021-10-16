using RazorLight;

namespace OpenStore.Infrastructure.Email.Templating;

public class RazorViewToStringRenderer : IRazorViewToStringRenderer
{
    private readonly RazorLightEngine _engine;

    public RazorViewToStringRenderer(RazorLightEngine engine)
    {
        _engine = engine;
    }

    public async Task<string> RenderViewToStringAsync<TModel>(string key, TModel model) => await _engine.CompileRenderAsync(key, model);
}