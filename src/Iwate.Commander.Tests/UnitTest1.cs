using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Iwate.Commander.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1Async()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AzureWebJobsStorage"] = "UseDevelopmentStorage=true",
        }).Build());

        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
        services.AddCommander(builder =>
        {
            builder
                .UseAzureBlobCommandStorage()
                //.UseInMemoryCommandStorage()
                .AddCommand<TestCommand>();
        });

        var provider = services.BuildServiceProvider();

        var commander = provider.GetRequiredService<Commander>();

        var user = Guid.NewGuid().ToString();
        string id;
        using (var payload = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(new 
        { 
            Arg1 = 1,
            Arg2 = 2,
        })))
        {
            id = await commander.EnqueueAsync(null, user, nameof(TestCommand), payload, CancellationToken.None);
        }

        var state = await commander.GetStateAsync(id, CancellationToken.None);

        var request = await commander.PeekAsync(null, CancellationToken.None);

        Assert.Equal(user, request.InvokedBy);

        state.Status = InvokeStatus.Processing;
        
        await commander.SetStateAsync(state, CancellationToken.None);

        await commander.InvokeAsync(request, CancellationToken.None);

        state.Status = InvokeStatus.Succeeded;
        await commander.SetStateAsync(state, CancellationToken.None);
        await commander.RemoveAsync(request, CancellationToken.None);
    }
}

public class TestCommand : ICommand
{
    private readonly InvokeRequest _request;
    private readonly ILogger _logger;
    private readonly string _args;
    public TestCommand(InvokeRequest request, ILoggerFactory loggerFactory)
    {
        _request = request;
        _logger = loggerFactory.CreateLogger<TestCommand>();
        using var reader = new StreamReader(request.Payload);
        _args = reader.ReadToEnd();
    }
    
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Id} {Command} {Args} Executing...", _request.Id, _request.Command, _args);

        return Task.CompletedTask;
    }
}
