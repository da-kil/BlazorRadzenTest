namespace SourceGeneratorTest;

public class TestCommand : ICommand<string>
{
    public string Message { get; init; }

    public TestCommand(string message)
    {
        Message = message;
    }
}