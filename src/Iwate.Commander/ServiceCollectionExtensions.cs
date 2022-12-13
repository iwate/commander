using Microsoft.Extensions.DependencyInjection;
using System;

namespace Iwate.Commander
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommander<TInvokeState>(this IServiceCollection services, Action<OptionsBuilder<TInvokeState>> build) where TInvokeState : IInvokeState, new()
        {
            var builder = new OptionsBuilder<TInvokeState>();
            build(builder);
            builder.Build(services);

            
            if (typeof(TInvokeState) == typeof(InvokeState))
            {
                services.AddSingleton<Commander>();
            }
            else
            {
                services.AddSingleton<Commander<TInvokeState>>();
            }
        }

        public static void AddCommander(this IServiceCollection services, Action<OptionsBuilder<InvokeState>> build)
        {
            AddCommander<InvokeState>(services, build);
        }
    }
}
