using Iwate.AzureFunctions.Middlewares.Singleton;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Iwate.Commander;

namespace Iwate.AzureFunctions.Middlewares.Commander;
public class PartitionLockMiddleware : IFunctionsWorkerMiddleware
{
    private readonly LockService _lockService;
    private readonly ICommandStoragePathResolver _pathResolver;
    private readonly ILogger _logger;
    public PartitionLockMiddleware(LockService lockService, ICommandStoragePathResolver pathResolver, ILogger<PartitionLockMiddleware> logger)
    {
        _lockService = lockService;
        _pathResolver = pathResolver;
        _logger = logger;
    }
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var workerAssembly = Assembly.GetEntryAssembly();

        var entryPointParts = context.FunctionDefinition.EntryPoint.Split(".");

        var workerTypeName = string.Join(".", entryPointParts[..^1]);
        var workerFunctionName = entryPointParts.Last();

        var workerType = workerAssembly?.GetType(workerTypeName)!;
        var workerFunction = workerType.GetMethod(workerFunctionName)!;

        if (
            workerFunction.GetCustomAttribute<PartitionLockAttribute>() is PartitionLockAttribute
            && context.BindingContext.BindingData.TryGetValue("BlobTrigger", out var value) 
            && value is not null 
            && value is string blobAbsolutePath
            && TryGetTrigger(blobAbsolutePath, out var trigger)
        )
        {
            _logger.LogTrace($"PartitionLock invocation '{context.InvocationId}' waiting for lock...");

            var lockName = _pathResolver.GetLockFileName(trigger?.Partition);

            await using (var @lock = await _lockService.Lock(lockName, CancellationToken.None))
            {
                _logger.LogTrace($"PartitionLock invocation '{context.InvocationId}' entered lock");

                context.SetCommanderTriggeredPartition(trigger?.Partition);

                try
                {
                    await next(context);
                    _logger.LogTrace($"PartitionLock invocation '{context.InvocationId}' released lock");
                }
                catch (Exception)
                {
                    _logger.LogTrace($"PartitionLock invocation '{context.InvocationId}' released lock");
                    throw;
                }
            }
        }
        else
        {
            await next(context);
        }
    }

    private bool TryGetTrigger(string blobAbsolutePath, out InvokeRequestBase? trigger)
    {
        trigger = null;

        var blobContainerNameAndBlobName = blobAbsolutePath.Split('/', 2);

        if (blobContainerNameAndBlobName.Length != 2)
        {
            _logger.LogInformation("PartitionLocked BlobTrigger is triggered but '{path}' can not be parsed", blobAbsolutePath);
            return false;
        }

        var blobName = blobContainerNameAndBlobName[1];

        if (!_pathResolver.TryParseQueue(blobName, out trigger))
        {
            _logger.LogInformation("PartitionLocked BlobTrigger is triggered but '{name}' can not be parsed", blobName);
            return false;
        }
        
        return true;
    }
}
