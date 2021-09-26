using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenStore.Data.OutBox
{
    public class OutBoxPollHost : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer _timer;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly TimeSpan _oneMinuteInterval = TimeSpan.FromMinutes(1);

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
                _oneMinuteInterval
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
            await _semaphore.WaitAsync();
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var outBox = scope.ServiceProvider.GetRequiredService<IOutBoxService>();
                await outBox.PushPendingMessages(int.MaxValue);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}