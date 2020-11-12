using System;
using System.Threading;
using System.Threading.Tasks;
using NCrontab;

namespace OpenStore.Infrastructure.Tasks.InMemory.Scheduled
{
    internal sealed class ScheduledJobDescriptor
    {
        public Func<IServiceProvider, CancellationToken, Task> WorkItem { get;}

        public CrontabSchedule Cron { get; }

        public DateTime NextRun { get; set; }
        public bool CanExecute => DateTime.Now >= NextRun;

        public ScheduledJobDescriptor(Func<IServiceProvider, CancellationToken, Task> workItem, string cronExpression)
        {
            if (cronExpression == null) throw new ArgumentNullException(nameof(cronExpression));
            WorkItem = workItem ?? throw new ArgumentNullException(nameof(workItem));
            Cron = CrontabSchedule.Parse(cronExpression);
            NextRun = Cron.GetNextOccurrence(DateTime.Now);
        }

        public void ScheduleNextRun() => NextRun = Cron.GetNextOccurrence(DateTime.Now);
    }
}