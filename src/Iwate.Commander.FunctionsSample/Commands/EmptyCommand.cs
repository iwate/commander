using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Iwate.Commander.FunctionsSample.Commands
{
    public class EmptyCommand : ICommand
    {
        private readonly ILogger _logger;
        public EmptyCommand(ILogger<EmptyCommand> logger)
        {
            _logger = logger;
        }
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Empty command is invoked.");
            return Task.CompletedTask;
        }
    }
}
