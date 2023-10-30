using Iwate.AzureFunctions.Middlewares.Commander;
using Iwate.Commander;
using Iwate.Commander.FunctionsSample.Commands;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((ctx, app) => {
        app.UseMiddleware<PartitionLockMiddleware>();
    })
    .ConfigureServices(services =>
    {
        services.AddPartitionLock();
        services.AddCommander(builder => {
            builder
                .UseAzureBlobCommandStorage()
                .AddCommand<EmptyCommand>();
        });
    })
    .Build();

await host.InitCommanderAsync();

host.Run();