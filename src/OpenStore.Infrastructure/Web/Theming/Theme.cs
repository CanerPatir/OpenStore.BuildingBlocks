namespace OpenStore.Infrastructure.Web.Theming;

public class Theme
{
    public static Theme Default { get; } = new Theme(nameof(Default));

    public Theme(string name)
    {
        Name = name;
    }

    public string Name { get; }
}