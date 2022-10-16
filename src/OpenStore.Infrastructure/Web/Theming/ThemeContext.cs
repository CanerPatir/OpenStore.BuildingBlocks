namespace OpenStore.Infrastructure.Web.Theming;

public class ThemeContext : IDisposable
{
    private bool _disposed;

    public string Id { get; } = Guid.NewGuid().ToString();
    public Theme Theme { get; }
    public IDictionary<string, object> Properties { get; }

    public ThemeContext(Theme theme)
    {
        Theme = theme ?? throw new ArgumentNullException(nameof(theme));
        Properties = new Dictionary<string, object>();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            foreach (var prop in Properties)
            {
                TryDisposeProperty(prop.Value as IDisposable);
            }

            TryDisposeProperty(Theme as IDisposable);
        }

        _disposed = true;
    }

    private void TryDisposeProperty(IDisposable obj)
    {
        if (obj == null)
            return;

        try
        {
            obj.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }
    }
}