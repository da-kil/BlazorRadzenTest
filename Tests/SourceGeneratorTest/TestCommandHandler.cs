namespace SourceGeneratorTest;

public class TestCommandHandler : ICommandHandler<TestCommand, string>
{
    public async Task<string> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return $"Handled: {command.Message}";
    }
}