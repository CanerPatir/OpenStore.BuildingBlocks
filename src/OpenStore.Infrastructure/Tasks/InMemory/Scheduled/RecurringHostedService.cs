using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace OpenStore.Infrastructure.Tasks.InMemory.Scheduled
{
    public abstract class RecurringHostedService : IHostedService
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly CrontabSchedule _schedule;
        private DateTime _nextRun;

        protected abstract string CronExpression { get; }
        protected ILogger Logger { get; }

        protected RecurringHostedService(IServiceProvider serviceProvider)
        {
            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().FullName);
            _schedule = CrontabSchedule.Parse(CronExpression);
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            // Store the task we are executing
            _executingTask = StartInternal(_stoppingCts.Token);

            // If the task is completed then return it,
            // this will bubble cancellation and failure to the caller
            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            return Task.CompletedTask;
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
                var now = DateTime.Now;
                if (now >= _nextRun)
                {
                    await ExecuteInternal(stoppingToken);
                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }

                await Task.Delay(3000, stoppingToken); //3 seconds delay
            } while (!stoppingToken.IsCancellationRequested);
        }

        private async Task ExecuteInternal(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Scheduled job executing");
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                await Execute(scope.ServiceProvider, stoppingToken);
                Logger.LogInformation("Scheduled job done");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Demystify(), "Scheduled job error");
            }
        }

        protected abstract Task Execute(IServiceProvider serviceProvider, CancellationToken token);
    }
}