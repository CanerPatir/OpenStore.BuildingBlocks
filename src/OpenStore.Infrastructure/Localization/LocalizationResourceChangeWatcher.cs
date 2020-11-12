using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenStore.Infrastructure.Localization
{
    public class LocalizationResourceChangeWatcher : IHostedService
    {
        private readonly IHostEnvironment _env;
        private readonly IReloadableStringLocalizerFactory _reloadableStringLocalizerFactory;
        private FileSystemWatcher _fsw;
        private readonly ILogger<LocalizationResourceChangeWatcher> _logger;
        private readonly OpenStoreRequestLocalizationOptions _options;

        public LocalizationResourceChangeWatcher(
            IHostEnvironment env,
            IReloadableStringLocalizerFactory reloadableStringLocalizerFactory, 
            IOptions<OpenStoreRequestLocalizationOptions> options,
            ILogger<LocalizationResourceChangeWatcher> logger)
        {
            _env = env;
            _reloadableStringLocalizerFactory = reloadableStringLocalizerFactory;
            _options = options.Value;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_env.IsDevelopment() || _options.Source == OpenStoreRequestLocalizationSource.EmbeddedSource)
            {
                return Task.CompletedTask;
            }
            
            _logger.LogInformation("Service Starting");
            var inputFolder = Path.Combine(_env.ContentRootPath, Path.GetDirectoryName(_options.ContentSourcePattern) ?? throw new Exception("Localization ContentSourcePattern is null."));

            if (!Directory.Exists(inputFolder))
            {
                _logger.LogWarning($"Please make sure the InputFolder [{inputFolder}] exists, then restart the service.");
                return Task.CompletedTask;
            }

            _logger.LogInformation($"Binding Events from Input Folder: {inputFolder}");
            _fsw = new FileSystemWatcher(inputFolder, "*.json")
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName |
                               NotifyFilters.LastAccess
            };

            _fsw.Changed += Input_OnChanged;

            _fsw.EnableRaisingEvents = true;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping LocalizationResourceChangeWatcher Service");
            _fsw.EnableRaisingEvents = false;

            return Task.CompletedTask;
        }

        private void Input_OnChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed) return;

            _logger.LogInformation($"InBound Change Event Triggered by [{e.FullPath}]");
            _reloadableStringLocalizerFactory.Reload();
            _logger.LogInformation("Done with Inbound Change Event");
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing Service");
            _fsw.Dispose();
        }
    }
}