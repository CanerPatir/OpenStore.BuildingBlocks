namespace OpenStore.Infrastructure.Localization.Resx
{
    public class OpenStoreResxLocalizationOptions : OpenStoreLocalizationOptions
    { 
        public string ResourcesPath { get; set; } = "Resources"; 
        public string SharedResourceName { get; set; } = "SharedResource"; 
        public string SharedResourceAssemblyName { get; set; } 
    }
}