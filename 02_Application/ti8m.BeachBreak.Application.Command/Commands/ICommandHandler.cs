namespace ti8m.BeachBreak.Application.Command.Commands;

public interface ICommandHandler<in TCommand, TResponse> where TCommand : class, ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
