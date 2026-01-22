namespace SourceGeneratorTest;

public interface ICommandHandler<in TCommand, TResponse> where TCommand : class, ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}