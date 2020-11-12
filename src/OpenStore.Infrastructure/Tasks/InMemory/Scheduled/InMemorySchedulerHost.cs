using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenStore.Infrastructure.Tasks.InMemory.Scheduled
{
    public class InMemorySchedulerHost : IHostedService
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly InMemoryTaskManager _taskManager;
        private readonly ILogger<InMemorySchedulerHost> _logger;

        public InMemorySchedulerHost(ITaskManager taskManager, IServiceProvider serviceProvider)
        {
            _taskManager = (InMemoryTaskManager)taskManager;
            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            _logger = serviceProvider.GetRequiredService<ILogger<InMemorySchedulerHost>>();
        }

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            // Store the task we are executing
            _executingTask = StartInternal(_stoppingCts.Token);

            // If the task is completed then return it,
            // this will bubble cancellation and failure to the caller
            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        private async Task StartInternal(CancellationToken stoppingToken)
        {
            do
            {
                foreach (var scheduledJobItem in _taskManager.ScheduledJobItems)
                {
                    if (!scheduledJobItem.CanExecute) continue;
                    TaskHelper.RunBgLong(() => ExecuteInternal(scheduledJobItem, stoppingToken));
                    scheduledJobItem.ScheduleNextRun();
                }

                await Task.Delay(1000, stoppingToken); //3 seconds delay
            } while (!stoppingToken.IsCancellationRequested);
        }

        private async Task ExecuteInternal(ScheduledJobDescriptor descriptor, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduled job executing");
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                await descriptor.WorkItem.Invoke(scope.ServiceProvider, stoppingToken);
                _logger.LogInformation("Scheduled job done");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Demystify(), "Scheduled job error");
            }
        }
    }
}