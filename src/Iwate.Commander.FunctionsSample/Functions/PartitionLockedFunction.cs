using Azure.Storage.Blobs;
using Iwate.AzureFunctions.Middlewares.Commander;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Iwate.Commander.FunctionsSample.Functions
{
    public class PartitionLockedFunction
    {
        private readonly Commander _commander;
        private readonly ILogger<PartitionLockedFunction> _logger;

        public PartitionLockedFunction(Commander commander, ILogger<PartitionLockedFunction> logger)
        {
            _commander = commander;
            _logger = logger;
        }

        [Function(nameof(PartitionLockedFunction))]
        [PartitionLock]
        public async Task Run(
            [BlobTrigger("commands/queue/{path}", Connection = "")] BlobClient blobClient,
            FunctionContext context,
            CancellationToken cancellationToken
        )
        {
            var partition = context.GetCommanderTriggeredPartition();

            var req = await _commander.PeekAsync(partition, cancellationToken);
            if (req == null)
            {
                _logger.LogInformation("ExecuteCommand is requested but {org}'s command queue is empty", partition);
                return;
            }

            var state = await _commander.GetStateAsync(req.Id, cancellationToken);

            state.Status = InvokeStatus.Processing;
            await _commander.SetStateAsync(state, cancellationToken);

            try
            {
                await _commander.InvokeAsync(req, cancellationToken);

                state.Status = InvokeStatus.Succeeded;
                await _commander.SetStateAsync(state, cancellationToken);
                await _commander.RemoveAsync(req, cancellationToken);
            }
            catch (Exception ex)
            {
                state.Status = InvokeStatus.Failed;
                await _commander.SetStateAsync(state, cancellationToken);
                await _commander.RemoveAsync(req, cancellationToken);
            }
        }
    }
}
