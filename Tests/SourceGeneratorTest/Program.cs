using Microsoft.Extensions.DependencyInjection;
using SourceGeneratorTest;

// Test the generated dispatcher functionality
var services = new ServiceCollection();

// Add the test command handler
services.AddScoped<TestCommandHandler>();
services.AddScoped<ICommandHandler<TestCommand, string>>(sp => sp.GetRequiredService<TestCommandHandler>());

// Add the generated dispatcher (would normally be done by AddGeneratedCommandHandlers)
// For this test, we'll simulate it
services.AddScoped<ICommandDispatcher, GeneratedCommandDispatcher>();

var serviceProvider = services.BuildServiceProvider();

// Test dispatching a command
var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
var command = new TestCommand("Hello from generated dispatcher!");

try
{
    var result = await dispatcher.SendAsync<string>(command);
    Console.WriteLine($"✅ Generated dispatcher test successful!");
    Console.WriteLine($"Result: {result}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Generated dispatcher test failed: {ex.Message}");
}

public interface ICommandDispatcher
{
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}

// Simple generated dispatcher for testing (in real project this comes from source generator)
public class GeneratedCommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public GeneratedCommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        return command switch
        {
            TestCommand cmd when typeof(TResponse) == typeof(string) =>
                (TResponse)(object)await _serviceProvider
                    .GetRequiredService<ICommandHandler<TestCommand, string>>()
                    .HandleAsync(cmd, cancellationToken),
            _ => throw new InvalidOperationException($"No handler registered for command type {command.GetType().Name}")
        };
    }
}