using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace OpenStore.Infrastructure.Web.Modularization;

internal static class ModuleConfigBuilder
{
    /// <summary>
    /// Merges module specific configuration and global configuration 
    /// </summary>
    /// <param name="environment">Host environment</param>
    /// <param name="globalConfiguration">Global configuration</param>
    /// <param name="settingFileNames">Module setting file names</param>
    /// <returns>Merged configuration</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IConfiguration Build(IHostEnvironment environment, IConfiguration globalConfiguration, IEnumerable<string> settingFileNames)
    {
        if (environment == null) throw new ArgumentNullException(nameof(environment));

        var configBuilder = new ConfigurationBuilder();

        var rootDir = AppContext.BaseDirectory;
        configBuilder.SetBasePath(rootDir);
        if (globalConfiguration != null)
        {
            configBuilder.AddConfiguration(globalConfiguration);
        }

        foreach (var settingFileName in settingFileNames)
        {
            var file = Path.Combine(rootDir, settingFileName);
            if (File.Exists(file))
            {
                configBuilder.AddJsonFile(settingFileName);
            }

            var envFile = Path.Combine(rootDir, $"{Path.GetFileNameWithoutExtension(settingFileName)}.{environment.EnvironmentName}{Path.GetExtension(settingFileName)}");
            if (File.Exists(envFile))
            {
                configBuilder.AddJsonFile(envFile);
            }
        }

        configBuilder.AddEnvironmentVariables();

        return configBuilder.Build();
    }
}