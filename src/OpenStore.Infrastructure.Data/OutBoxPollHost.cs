using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenStore.Infrastructure.Data
{
    public class OutBoxPollHost : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer _timer;
        private static readonly object Locker = new object();

        public OutBoxPollHost(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer
            (
                PushMessages,
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(1)
            );
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        
        private async void PushMessages(object state)
        {
            var hasLock = false;

            try
            {
                Monitor.TryEnter(Locker, ref hasLock);

                if (!hasLock)
                {
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var outBox = scope.ServiceProvider.GetRequiredService<IOutBoxService>();
                await outBox.PushPendingMessages(int.MaxValue);
            }
            finally
            {
                if (hasLock)
                {
                    Monitor.Exit(Locker);
                }
            }
        }
    }
}