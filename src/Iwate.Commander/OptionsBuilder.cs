using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Iwate.Commander
{
    public class OptionsBuilder<TInvokeState> where TInvokeState : IInvokeState, new()
    {
        private Func<IServiceProvider, ICommandStorage<TInvokeState>> _storageFactory = null;
        public OptionsBuilder<TInvokeState> UseStorage(Func<IServiceProvider, ICommandStorage<TInvokeState>> factory)
        {
            _storageFactory = factory;
            return this;
        }
        public OptionsBuilder<TInvokeState> UseAzureBlobCommandStorage(
            string connectionStringName = "AzureWebJobsStorage", 
            string containerName = "commands", 
            string queueDir = "queue", 
            string stateDir = "state")
        {
            return UseStorage(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var pathResolver = new CommandStoragePathResolver(queueDir, stateDir);
                return new AzureBlobCommandStorage<TInvokeState>(configuration[connectionStringName], containerName, pathResolver);
            });
        }

        public OptionsBuilder<TInvokeState> UseInMemoryCommandStorage()
        {
            return UseStorage(provider =>
            {
                return new InMemoryCommandStorage<TInvokeState>();
            });
        }

        private readonly CommandRegistry _registry = new CommandRegistry();
        public OptionsBuilder<TInvokeState> AddCommand<TCommand>() where TCommand : ICommand
        {
            _registry.Append<TCommand>();
            return this;
        }
        internal void Build(IServiceCollection services)
        {
            if (_storageFactory == null)
            {
                throw new InvalidOperationException("Command storage is not configured");
            }

            services.AddScoped<InvokeRequestAccessor>();
            services.AddScoped(provider => provider.GetRequiredService<InvokeRequestAccessor>().Get());

            services.AddSingleton<ICommandRegistry>(_registry);
            foreach(var type in _registry.Values)
            {
                services.AddScoped(type);
            }
            services.AddSingleton(_storageFactory);
            services.AddSingleton<ICommandInvoker, CommandInvoker>();
        }
    }
}
