using Iwate.AzureFunctions.Middlewares.Singleton;
using Microsoft.Extensions.DependencyInjection;

namespace Iwate.AzureFunctions.Middlewares.Commander
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPartitionLock(this IServiceCollection services)
        {
            services.AddSingleton<LockService>();
        }
    }
}
