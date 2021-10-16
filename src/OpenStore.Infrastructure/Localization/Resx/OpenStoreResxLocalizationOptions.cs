using System.Reflection;

namespace OpenStore.Infrastructure.Localization.Resx;

public class OpenStoreResxLocalizationOptions : OpenStoreLocalizationOptions
{ 
    public string ResourcesPath { get; set; } = "Resources"; 
    public string SharedResourceName { get; set; } = "SharedResource"; 
    public Assembly Assembly { get; set; } 
}