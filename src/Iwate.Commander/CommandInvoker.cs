using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iwate.Commander
{
    internal class CommandInvoker : ICommandInvoker
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICommandRegistry _registry;
        private readonly ILogger _logger;
        public CommandInvoker(IServiceProvider serviceProvider, ICommandRegistry registry, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _registry = registry;
            _logger = loggerFactory.CreateLogger<CommandInvoker>();
        }
        public async Task InvokeAsync(InvokeRequest request, CancellationToken cancellationToken)
        {
            var scope = _serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<InvokeRequestAccessor>().Set(request);

            if (!_registry.TryLookup(request.Command, out var type))
            {
                _logger.LogWarning("Unknown Command: {Command}", request.Command);
                return;
            }

            var command = scope.ServiceProvider.GetService(type) as ICommand;
            if (command == null)
            {
                _logger.LogWarning("Invalid Command: {Command}", request.Command);
                return;
            }

            await command.ExecuteAsync(cancellationToken);

        }
    }
}
