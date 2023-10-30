using Iwate.AzureFunctions.Middlewares.Singleton;
using Iwate.Commander;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Iwate.AzureFunctions.Middlewares.Commander
{
    public static class HostExtensions
    {
        public static Task InitCommanderAsync<TInvokeState>(this IHost host) where TInvokeState : IInvokeState
            => InitCommanderAsync<TInvokeState>(host, null);
        public static async Task InitCommanderAsync<TInvokeState>(this IHost host, Func<Task>? additionalInitAction) where TInvokeState : IInvokeState
        {
            var commander = host.Services.GetRequiredService<Commander<TInvokeState>>();
            var pathResolver = host.Services.GetRequiredService<ICommandStoragePathResolver>();
            var lockService = host.Services.GetRequiredService<LockService>();
            var lockName = pathResolver.GetLockFileName(null);

            await using var @lock = await lockService.Lock(lockName, CancellationToken.None);
            
            await commander.InitAsync();

            if (additionalInitAction != null)
            {
                await additionalInitAction();
            }
        }

        public static Task InitCommanderAsync(this IHost host)
            => InitCommanderAsync(host, null);
        public static async Task InitCommanderAsync(this IHost host, Func<Task>? additionalInitAction)
        {
            var commander = host.Services.GetRequiredService<Iwate.Commander.Commander>();
            var pathResolver = host.Services.GetRequiredService<ICommandStoragePathResolver>();
            var lockService = host.Services.GetRequiredService<LockService>();
            var lockName = pathResolver.GetLockFileName(null);

            await using var @lock = await lockService.Lock(lockName, CancellationToken.None);

            await commander.InitAsync();

            if (additionalInitAction != null)
            {
                await additionalInitAction();
            }
        }
    }
}
