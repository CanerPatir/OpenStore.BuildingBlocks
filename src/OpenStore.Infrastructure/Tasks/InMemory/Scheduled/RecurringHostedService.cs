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
    internal class RecurringHostedService<TJob> : BackgroundService
        where TJob: IRecurringJob
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new();
        
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;
        private readonly CrontabSchedule _schedule;
        private DateTime _nextRun;
        
        public RecurringHostedService(string cronExpression, IServiceScopeFactory serviceScopeFactory, ILogger<RecurringHostedService<TJob>> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _schedule = CrontabSchedule.Parse(cronExpression);
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
            _logger.LogInformation("Scheduled job executing");
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<TJob>();
                await job.Execute(stoppingToken);
                _logger.LogInformation("Scheduled job done");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Demystify(), "Scheduled job error");
            }
        }
 
    }
}